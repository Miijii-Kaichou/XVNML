#nullable enable

using System;
using System.Linq;
using System.Threading;
using XVNML.Core.Dialogue;
using XVNML.Core.Macros;

using Timer = System.Timers.Timer;

namespace XVNML.Utility.Dialogue
{
    public delegate void DialogueWriterCallback(DialogueWriterProcessor sender);

    public static class DialogueWriter
    {
        public static DialogueWriterCallback?[]? OnLineStart;
        public static DialogueWriterCallback?[]? OnLineSubstringChange;
        public static DialogueWriterCallback?[]? OnLinePause;
        public static DialogueWriterCallback?[]? OnNextLine;
        public static DialogueWriterCallback?[]? OnDialogueFinish;

        // Cast-Specific Callbacks
        public static DialogueWriterCallback?[]? OnCastChange;
        public static DialogueWriterCallback?[]? OnCastExpressionChange;
        public static DialogueWriterCallback?[]? OnCastVoiceChange;

        // Prompt-Specific Callbacks
        public static DialogueWriterCallback?[]? OnPrompt;
        public static DialogueWriterCallback?[]? OnPromptResonse;

        public static int TotalProcesses => WriterProcesses!.Length;

        internal static DialogueWriterProcessor?[]? WriterProcesses { get; private set; }
        internal static bool[]? WaitingForUnpauseCue { get; private set; }

        private static Thread? _writingThread;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static bool IsInitialized = false;
        private static Timer[]? ProcessTimers;
        private static bool[]? ProcessStalling;


        private const int DefaultTotalChannelsAllocated = 12;

        /// <summary>
        /// Allocate a total amount of Dialogue Process prior to
        /// write start up. Allocations are immutable once set.
        /// </summary>
        /// <param name="totalChannels"></param>
        public static void AllocateChannels(int totalChannels = DefaultTotalChannelsAllocated)
        {
            if (IsInitialized) return;

            totalChannels = totalChannels < 1 ? DefaultTotalChannelsAllocated : totalChannels;

            WriterProcesses = new DialogueWriterProcessor[totalChannels];
            OnLineStart = new DialogueWriterCallback[totalChannels];
            OnLineSubstringChange = new DialogueWriterCallback[totalChannels];
            OnLinePause = new DialogueWriterCallback[totalChannels];
            OnNextLine = new DialogueWriterCallback[totalChannels];
            OnDialogueFinish = new DialogueWriterCallback[totalChannels];
            OnCastChange = new DialogueWriterCallback[totalChannels];
            OnCastExpressionChange = new DialogueWriterCallback[totalChannels];
            OnCastVoiceChange = new DialogueWriterCallback[totalChannels];
            OnPrompt = new DialogueWriterCallback[totalChannels];
            OnPromptResonse = new DialogueWriterCallback[totalChannels];

            ProcessTimers = new Timer[totalChannels];

            ProcessStalling = new bool[totalChannels];
            WaitingForUnpauseCue = new bool[totalChannels];
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script)
        {
            if (WriterProcesses == null || WriterProcesses.Length == 0)
                AllocateChannels();

            for (int i = 0; i < WriterProcesses!.Length; i++)
            {
                if (WriterProcesses[i] == null)
                {
                    Write(script, i);
                    return;
                }
            }
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script, int channel)
        {
            if (WriterProcesses == null || WriterProcesses.Length == 0)
                AllocateChannels(channel);

            var newProcess = DialogueWriterProcessor.Initialize(script, channel);
            if (newProcess == null) { return; }

            if (IsInitialized == false) Initialize();
            WriterProcesses![channel] = newProcess;
        }


        private static void Initialize()
        {
            cancellationTokenSource = new CancellationTokenSource();
            IsInitialized = true;
            _writingThread = new Thread(new ParameterizedThreadStart(WriterThread))
            {
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true
            };
            MacroInvoker.Init();
            _writingThread.Start(cancellationTokenSource);
        }

        private static void WriterThread(object obj)
        {
            CancellationToken cancelationToken = ((CancellationTokenSource)obj).Token;
            while (IsInitialized && !cancelationToken.IsCancellationRequested)
            {
                DoConcurrentDialogueProcesses();
                Thread.Sleep(10);
            }
        }

        private static void DoConcurrentDialogueProcesses()
        {
            var processes = WriterProcesses!.Where(process => process != null).ToList();
            foreach (var process in processes)
            {
                if (process == null) return;
                lock (process.processLock)
                {
                    ProcessLine(process!);
                }
            };
        }

        private static void ProcessLine(DialogueWriterProcessor process)
        {
            if (process == null) return;

            lock (process.processLock)
            {
                int id = process.ID;

                process.UpdatePrevious();

                if (process.lineProcesses.Count == 0 && process.currentLine == null) return;
                if (IsRestricting(process)) return;

                
                if (process.currentLine == null)
                {
                    process.lineProcesses.TryDequeue(out process.currentLine);
                    OnLineStart?[id]?.Invoke(process);
                }

                if (process.linePosition > process.currentLine.Content?.Length - 1)
                {
                    if (CheckForRetries(process)) return;
                    if (IsRestricting(process)) return;
                    process.IsPaused = true;
                    WriterProcesses![id] = process;
                    OnLineSubstringChange?[id]?.Invoke(process);
                    OnLinePause?[id]?.Invoke(process!);
                    return;
                }

                if (CheckForRetries(process))
                {                  
                    return;
                }
                Next(process);
                process.currentLine!.ReadPosAndExecute(process);
                UpdateSubString(process);
                Yield(process);
            }
        }

        private static bool CheckForRetries(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (MacroInvoker.RetriesQueued[process.ID])
                {
                    MacroInvoker.AttemptRetries(process);
                    UpdateSubString(process);
                    return true;
                }
                return false;
            }
        }

        private static void Next(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (IsRestricting(process)) return;
                process.linePosition++;
            }
        }

        private static void UpdateSubString(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {           
                var id = process.ID;

                if (process.linePosition == -1) return;
                if (process.linePosition > process.currentLine?.Content?.Length - 1) return;
                
                if (IsRestricting(process)) return;

                process.CurrentLetter = process.currentLine?.Content?[process.linePosition];
                WriterProcesses![id] = process;
                OnLineSubstringChange?[id]?.Invoke(process);
            }
        }

        internal static bool IsRestricting(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (ProcessStalling![process.ID]) return true;
                if (process.IsPaused) return true;
                if (process.WasControlledPause) return true;
                if (process.IsOnDelay) return true;
                if (WaitingForUnpauseCue![process.ID]) return true;

                return false;
            }
        }

        private static void Yield(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                ProcessStalling![process.ID] = true;
                ProcessTimers![process.ID] ??= new Timer(process.ProcessRate);
                ProcessTimers![process.ID].Interval = process.ProcessRate;
                if (ProcessTimers![process.ID] != null)
                {
                    ProcessTimers![process.ID].Enabled = true;
                }
                ProcessTimers[process.ID].AutoReset = false;
                ProcessTimers[process.ID].Elapsed += (s, e) =>
                {
                    s = process.ID;
                    DialogueWriter_Elapsed(s, e);
                };
                ProcessTimers[process.ID].Enabled = true;
            }
        }

        private static void DialogueWriter_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessStalling![(int)sender] = false;
        }

        public static void ShutDown()
        {
            cancellationTokenSource.Cancel();
        }

        public static void MoveNextLine(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (process.WasControlledPause)
                {
                    process.Unpause();
                    WaitingForUnpauseCue![process.ID] = false;
                    WriterProcesses![process.ID] = process;
                    return;
                }
                if (process.IsPaused == false) return;
                process.IsPaused = false;
                if (process.lineProcesses.Count == 0)
                {
                    // That was the last dialogue
                    OnDialogueFinish?[process!.ID]?.Invoke(process);
                    Reset(process);
                    return;
                }
                OnNextLine?[process.ID]?.Invoke(process);
                Reset(process);
            }
        }

        private static void Reset(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                process.currentLine = null;
                process.CurrentLetter = null;
                process.linePosition = -1;
                process.Clear();
            }
        }
    }
}