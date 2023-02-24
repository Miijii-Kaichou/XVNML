#nullable enable

using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using XVNML.Core.Dialogue;
using System.Text;
using Timer = System.Timers.Timer;
using static System.Net.Mime.MediaTypeNames;

namespace XVNML.Utility.Dialogue
{
    public delegate void DialogueWriterCallback(DialogueWriterProcessor sender);

    public static class DialogueWriter
    {
        public static DialogueWriterCallback? OnLineStart;
        public static DialogueWriterCallback? OnLineSubstringChange;
        public static DialogueWriterCallback? OnLinePause;
        public static DialogueWriterCallback? OnNextLine;
        public static DialogueWriterCallback? OnDialogueFinish;

        public static int TotalProcesses => WriterProcesses.Length;
        
        internal static DialogueWriterProcessor?[] WriterProcesses { get; private set; }
        internal static bool WaitingForUnpauseQueue { get; private set; }

        private static Thread? dialogueWritingThread;
        private static CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();
        private static bool IsInitialized = false;
        private static Timer[] ProcessTimers;
        private static bool[] ProcessStalling;

        private const int DefaultTotalChannelsAllocated = 12;

        /// <summary>
        /// Allocate a total amount of Dialogue Process prior to
        /// write start up. Allocations are immutable once set.
        /// </summary>
        /// <param name="totalChannels"></param>
        public static void AllocateChannels(int totalChannels = DefaultTotalChannelsAllocated)
        {
            if (IsInitialized) return;
            totalChannels = totalChannels == 0 ? DefaultTotalChannelsAllocated : totalChannels;
            WriterProcesses = new DialogueWriterProcessor[totalChannels];
            ProcessTimers = new Timer[totalChannels];
            ProcessStalling = new bool[totalChannels];
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script)
        {
            if (WriterProcesses == null || WriterProcesses.Length == 0)
                AllocateChannels();

            int i = 0;

            for(;i < WriterProcesses.Length;i++)
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

            WriterProcesses[channel] = newProcess;
        }


        private static void Initialize()
        {
            cancelationTokenSource = new CancellationTokenSource();
            IsInitialized = true;
            dialogueWritingThread = new Thread(new ParameterizedThreadStart(WriterThread));
            dialogueWritingThread.Start(cancelationTokenSource);
        }

        private static void WriterThread(object obj)
        {
            CancellationToken cancelationToken = ((CancellationTokenSource)obj).Token;
            while (IsInitialized && !cancelationToken.IsCancellationRequested)
            {
                Parallel.ForEach<DialogueWriterProcessor>(WriterProcesses!, ProcessLine);
                Thread.Sleep(100);
            }
        }

        private static void ProcessLine(DialogueWriterProcessor process)
        {
            // We don't need to read the dialogue
            // if the line stack is 0;
            if (process == null) return;
            if (process.lineProcesses.Count == 0) return;
            if (process.isPaused) return;

            if (process.currentLine == null)
            {
                Console.Clear();
                process.lineProcesses.TryDequeue(out process.currentLine);
                OnLineStart?.Invoke(process);
            }

            // We can now add to the currently displayed screen
            process.linePosition = -1;
            while (process.isPaused == false)
            {
                if (ProcessStalling[process.ID]) continue;

                // Don't do anything if you are on delay
                if (process.IsOnDelay || WaitingForUnpauseQueue)
                {
                    Yield(process);
                    continue;
                }

                if (WaitingForUnpauseQueue == false && process.WasControlledPause)
                {
                    WaitingForUnpauseQueue = true;
                    OnLinePause?.Invoke(process!);
                    continue;
                }

                Next();

                process.currentLine!.ReadPosAndExecute(process);

                if (process.linePosition > process.currentLine.Content?.Length - 1)
                {
                    process.isPaused = true;
                    WriterProcesses[process.ID] = process;
                    OnLineSubstringChange?.Invoke(process);
                    OnLinePause?.Invoke(process!);
                    return;
                }

                UpdateSubString(process);
            }

            void Next()
            {
                if (process.IsOnDelay || WaitingForUnpauseQueue) return;
                process.linePosition++;
            }

            void UpdateSubString(DialogueWriterProcessor process)
            {
                if (process.IsOnDelay) return;
                if (WaitingForUnpauseQueue) return;
                if (process.IsOnDelay || process.WasControlledPause) return;
                process.CurrentLetter = process.currentLine?.Content?[process.linePosition];
                WriterProcesses[process.ID] = process;
                OnLineSubstringChange?.Invoke(process);
                Yield(process);
            }
        }

        private static void Yield(DialogueWriterProcessor process)
        {
            ProcessStalling[process.ID] = true;
            ProcessTimers[process.ID] = new Timer(process.ProcessRate);
            ProcessTimers[process.ID].AutoReset = false;
            ProcessTimers[process.ID].Elapsed += (s, e) =>
            {
                s = process.ID;
                DialogueWriter_Elapsed(s, e);
            };
            ProcessTimers[process.ID].Enabled = true;
        }

        private static void DialogueWriter_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessStalling[(int)sender] = false;
        }

        public static void ShutDown()
        {
            cancelationTokenSource.Cancel();
        }

        public static void MoveNextLine(DialogueWriterProcessor process)
        {
            if (process.WasControlledPause)
            {
                WaitingForUnpauseQueue = false;
                process.Unpause();
                WriterProcesses[process.ID] = process;
                return;
            }
            if (process.isPaused == false) return;
            if (process.lineProcesses.Count == 0)
            {
                // That was the last dialogue
                OnDialogueFinish?.Invoke(process);
                Reset(process);
                return;
            }
            OnNextLine?.Invoke(process);
            Reset(process);
        }

        private static void Reset(DialogueWriterProcessor process)
        {
            
            process.Clear();
            process.currentLine = null;
            process.CurrentLetter = null;
            process.isPaused = false;
        }

        #region Extensions
        public static string Feed(this string text, DialogueWriterProcessor process)
        {
            if (IsInitialized == false) Initialize();
            if (WaitingForUnpauseQueue) return text;
            if (process.IsOnDelay || process.WasControlledPause) return text;
            text = process.DisplayingContent!;
            return text;
        }

        #endregion
    }
}
