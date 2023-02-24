#nullable enable

using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using XVNML.Core.Dialogue;

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

        private static Thread? dialogueWritingThread;
        private static CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();
        private static bool IsInitialized = false;
        private static bool waitingForUnpauseCue;

        internal static DialogueWriterProcessor?[] WriterProcesses { get; private set; }

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
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script)
        {
            if (WriterProcesses == null || WriterProcesses.Length == 0)
                AllocateChannels();
            Write(script, Array.IndexOf(WriterProcesses, WriterProcesses.Where(dwp => dwp == null).Single()));
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
                Thread.Sleep(10);
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
                // Don't do anything if you are on delay
                if (process.IsOnDelay || waitingForUnpauseCue)
                {
                    Thread.Sleep((int)process.processRate);
                    continue;
                }

                if (waitingForUnpauseCue == false && process.WasControlledPause)
                {
                    waitingForUnpauseCue = true;
                    WriterProcesses[process.ProcessID] = process;
                    OnLinePause?.Invoke(process!);
                    continue;
                }

                Next();

                process.currentLine!.ReadPosAndExecute(process);


                if (process.linePosition > process.currentLine.Content?.Length - 1)
                {
                    process.isPaused = true;
                    WriterProcesses[process.ProcessID] = process;
                    OnLineSubstringChange?.Invoke(process);
                    OnLinePause?.Invoke(process!);
                    return;
                }

                UpdateSubString(process);
            }

            void Next()
            {
                if (process.IsOnDelay) return;
                process.linePosition++;
            }

            void UpdateSubString(DialogueWriterProcessor process)
            {
                if (process.IsOnDelay) return;
                process.CurrentLetter = process.currentLine.Content?[process.linePosition];
                WriterProcesses[process.ProcessID] = process;
                OnLineSubstringChange?.Invoke(process);
                Thread.Sleep((int)process.processRate);
            }
        }


        public static void ShutDown()
        {
            cancelationTokenSource.Cancel();
        }

        public static void MoveNextLine(DialogueWriterProcessor process)
        {
            if (process.WasControlledPause)
            {
                waitingForUnpauseCue = false;
                process.Unpause();
                WriterProcesses[process.ProcessID] = process;
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
            if (process.IsOnDelay || process.WasControlledPause) return text;
            text = process.DisplayingContent!;
            return text;
        }

        #endregion
    }
}
