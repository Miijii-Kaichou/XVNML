﻿#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XVNML.Core.Dialogue;
using XVNML.Core.Dialogue.Enums;
using XVNML.Core.Macros;
using XVNML.Utilities.Diagnostics;

using Timer = System.Timers.Timer;

namespace XVNML.Utilities.Dialogue
{
    public delegate void DialogueWriterCallback(DialogueWriterProcessor sender);

    public static class DialogueWriter
    {
        // Root Scope Identifier
        public static string?[]? RootScopeIdentifierSet;

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

        // Blocking
        public static DialogueWriterCallback?[]? OnChannelBlock;
        public static DialogueWriterCallback?[]? OnChannelUnblock;

        public static int TotalProcesses => WriterProcesses!.Length;
        public static Stack<string>? ResponseStack { get; private set; } = new Stack<string>();

        internal static DialogueWriterProcessor?[]? WriterProcesses { get; private set; }
        internal static bool[]? WaitingForUnpauseCue { get; private set; }
        internal static int ThreadInterval { get; private set; }
        internal static bool[]? IsChannelBlocked { get; private set; }

        private static bool IsInitialized = false;
        private static bool[]? ProcessStalling;
        private static Thread? _writingThread;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private static Timer[]? ProcessTimers;
        private static List<DialogueWriterProcessor?>? WriterProcessesCache;

        private const int DefaultTotalChannelsAllocated = 12;
        private const int DefaultInterval = 1;
        private const int SingleChannel = DefaultInterval;
        private const int NoLength = 0;
        private const int TenthOfASecond = 10;
        private const int NoDefinedIndex = -1;

        /// <summary>
        /// Allocate a total amount of Dialogue Process prior to
        /// write start up. Allocations are immutable once set.
        /// </summary>
        /// <param name="totalChannels"></param>
        public static void AllocateChannels(int totalChannels = DefaultTotalChannelsAllocated)
        {
            if (IsInitialized) return;
             CreateAnew(totalChannels);
        }

        private static void  CreateAnew(int totalChannels)
        {
            totalChannels = totalChannels < SingleChannel ? DefaultTotalChannelsAllocated : totalChannels;

            RootScopeIdentifierSet = new string[totalChannels];
            
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

            OnChannelBlock = new DialogueWriterCallback[totalChannels];
            OnChannelUnblock = new DialogueWriterCallback[totalChannels];

            ProcessTimers = new Timer[totalChannels];

            ProcessStalling = new bool[totalChannels];
            WaitingForUnpauseCue = new bool[totalChannels];
            IsChannelBlocked = new bool[totalChannels];

        }

        public static void SetThreadInterval(int interval = DefaultInterval)
        {
            if (interval < DefaultInterval)
            {
                XVNMLLogger.LogWarning("Thread Interval must be greater than 1", typeof(DialogueWriter));
                return;
            }

            ThreadInterval = interval * TenthOfASecond;
            XVNMLLogger.Log($"Thread Interval set to {interval}", typeof(DialogueWriter));
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script)
        {
            if (WriterProcesses == null || WriterProcesses.Length == NoLength)
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
            if (WriterProcesses == null || WriterProcesses.Length == NoLength)
                AllocateChannels(channel);

            var newProcess = DialogueWriterProcessor.Initialize(script, channel);
            if (newProcess == null) { return; }

            newProcess.processLock = new object();
            if (IsInitialized == false) Initialize();
            
            WriterProcesses![channel] = newProcess;
            WriterProcessesCache = null;
        }

        public static void ShutDown()
        {
            WriterProcesses = null;
            WriterProcessesCache = null;
            cancellationTokenSource.Cancel();
            IsInitialized = false;
        }

        public static void MoveNextLine(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                process.ResetPass();
                if (process.WasControlledPause)
                {
                    process.Unpause();
                    WaitingForUnpauseCue![process.ID] = false;
                    WriterProcesses![process.ID] = process;
                    return;
                }
                if (process.IsPaused == false) return;
                process.IsPaused = false;

                process.currentLine?.Purify();
                OnNextLine?[process.ID]?.Invoke(process);
                ResetProcess(process);
            }
        }
        
        public static void Block(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                IsChannelBlocked![process.ID] = true;
                OnChannelBlock![process.ID]?.Invoke(process);
            }
        }

        public static void UnBlock(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                IsChannelBlocked![process.ID] = false;
                OnChannelUnblock![process.ID]?.Invoke(process);
            }
        }

        internal static bool IsRestricting(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (IsChannelBlocked![process.ID]) return true;
                if (ProcessStalling![process.ID]) return true;
                if (process.IsPaused) return true;
                if (process.inPrompt) return true;
                if (process.WasControlledPause) return true;
                if (process.IsOnDelay) return true;
                if (WaitingForUnpauseCue![process.ID]) return true;

                return false;
            }
        }

        private static void Initialize()
        {
            cancellationTokenSource = new CancellationTokenSource();
            IsInitialized = true;
            _writingThread = new Thread(new ParameterizedThreadStart(WriterThread))
            {
                Priority = ThreadPriority.AboveNormal
            };

            MacroInvoker.Init();

            if (ThreadInterval != 1) SetThreadInterval();

            _writingThread.Start(cancellationTokenSource);
        }

        private static void WriterThread(object obj)
        {
            CancellationToken cancelationToken = ((CancellationTokenSource)obj).Token;

            while (IsInitialized && !cancelationToken.IsCancellationRequested)
            {
                DoConcurrentDialogueProcesses();
                Thread.Sleep(ThreadInterval);
            }
        }

        private static void DoConcurrentDialogueProcesses()
        {
            WriterProcessesCache ??= WriterProcesses!.Where(process => process != null).ToList();
            foreach (var process in WriterProcessesCache)
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

                if (process.AtEnd && process.currentLine == null)
                {
                    // That was the last dialogue
                    ResetProcess(process);
                    WriterProcesses![id] = null;
                    OnDialogueFinish?[process!.ID]?.Invoke(process);
                    return;
                }

                if (IsRestricting(process)) return;


                if (process.currentLine == null)
                {
                    process.UpdateProcess();
                    process.CurrentCastInfo = process.currentLine?.InitialCastInfo;
                    OnLineStart?[id]?.Invoke(process);
                }

                if (process.cursorIndex > process.currentLine?.Content?.Length - 1)
                {
                    if (CheckForRetries(process)) return;
                    if (IsRestricting(process)) return;

                    WriterProcesses![id] = process;
                    OnLineSubstringChange?[id]?.Invoke(process);

                    if (process.currentLine?.SignatureInfo?.CurrentRole == Role.Interrogative && process.inPrompt == false && process.Response == null)
                    {
                        process.inPrompt = true;
                        OnPrompt?[id]?.Invoke(process);
                        return;
                    }

                    if (process.currentLine?.SignatureInfo?.CurrentRole == Role.Interrogative &&
                        process.Response != null)
                    {
                        ResponseStack?.Push(process.Response);
                        process.Response = null;
                        ResetProcess(process);
                        OnPromptResonse?[id]?.Invoke(process);
                        process.inPrompt = false;
                        return;
                    }

                    process.IsPaused = true;
                    OnLinePause?[id]?.Invoke(process!);

                    return;
                }


                if (CheckForRetries(process)) return;

                Next(process);
                process.currentLine?.ReadPosAndExecute(process, RootScopeIdentifierSet![process.ID]!);
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
                    return true;
                }
                UpdateSubString(process);
                return false;
            }
        }

        private static void Next(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (IsRestricting(process)) return;
                process.cursorIndex++;
            }
        }

        private static void UpdateSubString(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                var id = process.ID;

                if (process.cursorIndex == NoDefinedIndex) return;
                if (process.cursorIndex > process.currentLine?.Content?.Length - 1) return;

                if (IsRestricting(process)) return;

                process.CurrentLetter = process.currentLine?.Content?[process.cursorIndex];

                WriterProcesses![id] = process;
                OnLineSubstringChange?[id]?.Invoke(process);
            }
        }

        private static void Yield(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (IsChannelBlocked![process.ID]) return;

                ProcessStalling![process.ID] = true;
                ProcessTimers![process.ID] ??= new Timer(process.ProcessRate);
                ProcessTimers![process.ID].Interval = (double)process.ProcessRate;

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


        private static void ResetProcess(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                process.currentLine = null;
                process.CurrentLetter = null;
                process.cursorIndex = NoDefinedIndex;
                process.Clear();
            }
        }
    }
}