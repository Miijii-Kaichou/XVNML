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
        public static DialogueWriterCallback? OnLineFinished;
        public static DialogueWriterCallback? OnNextLine;
        public static DialogueWriterCallback? OnDialogueFinish;

        private static Thread? dialogueWritingThread;
        private static CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();
        private static bool IsInitialized = false;

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
            WriterProcesses = new DialogueWriterProcessor[totalChannels];
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script)
        {
            if (WriterProcesses == null) AllocateChannels();
            Write(script, Array.IndexOf(WriterProcesses, WriterProcesses.Where(dwp => dwp == null).Single()));
        }

        /// <summary>
        /// Begin the writing process of a dialogue.
        /// </summary>
        /// <param name="script"></param>
        public static void Write(DialogueScript script, int channel)
        {
            if (WriterProcesses == null) AllocateChannels(channel);
            
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
            if (process.waitingForUserInput) return;

            if (process.currentLine == null)
            {
                Console.Clear();
                process.lineProcesses.TryDequeue(out process.currentLine);
                OnLineStart?.Invoke(process);
            }

            // We can now add to the currently displayed screen
            process.linePosition = -1;
            while (process.waitingForUserInput == false)
            {
                Next();
                if (process.linePosition > process.currentLine.Content?.Length - 1)
                {
                    process.waitingForUserInput = true;
                    WriterProcesses[process.ProcessID] = process;
                    OnLineFinished?.Invoke(process!);
                    return;
                }

                process.currentLine!.ReadPosAndExecute(process.linePosition);
                process.CurrentLetter = process.currentLine.Content?[process.linePosition];
                WriterProcesses[process.ProcessID] = process;
                OnLineSubstringChange?.Invoke(process);
                Thread.Sleep((int)process.processRate);
            }

            void Next() => process.linePosition++;

        }

        public static void ShutDown()
        {
            cancelationTokenSource.Cancel();
        }

        public static void MoveNextLine(DialogueWriterProcessor process)
        {
            if (process.waitingForUserInput == false) return;
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
            process.currentLine = null;
            process.CurrentLetter = null;
            process.waitingForUserInput = false;
        }

        #region Extensions
        public static void Feed(this ref ReadOnlySpan<char> text, DialogueWriterProcessor process)
        {
            if (IsInitialized == false) Initialize();
            text = process.CurrentLetter?.ToString()!;
        }

        #endregion
    }
}
