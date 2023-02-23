#nullable enable

using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using XVNML.Core.Dialogue;

namespace XVNML.Utility.Dialogue
{
    public delegate void DialogueWriterCallback(DialogueLine sender);

    public static class DialogueWriter
    {
        public static DialogueWriterCallback? OnLineStart;
        public static DialogueWriterCallback? OnLineSubstringChange;
        public static DialogueWriterCallback? OnLineFinished;
        public static DialogueWriterCallback? OnNextLine;
        public static DialogueWriterCallback? OnDialogueFinish;

        private static Thread? dialogueWritingThread;
        private static bool IsInitialized = false;

        private static ConcurrentQueue<DialogueLine> lineProcesses = new ConcurrentQueue<DialogueLine>();
        private static DialogueLine? currentLine;

        private static char? currentLetter = null;
        private static bool waitingForUserInput;
        private static int linePosition;

        internal static uint TextRate { get; private set; } = 20;

        public static void Run(DialogueScript script)
        {
            if (IsInitialized == false) Initialize();

            //TODO: read script, and get lines
            foreach(DialogueLine line in script.Lines)
            {
                lineProcesses.Enqueue(line);
            }
        }

        private static void Initialize()
        {
            IsInitialized = true;
            dialogueWritingThread = new Thread(new ParameterizedThreadStart(WriterThread));
            dialogueWritingThread.Start();
        }

        private static void WriterThread(object data)
        {
            while(IsInitialized)
            {
                ProcessLine();
                Thread.Sleep(10);
            }
        }

        private static void ProcessLine()
        {
            // We don't need to read the dialogue
            // if the line stack is 0;
            if (lineProcesses.Count == 0) return;
            if (waitingForUserInput) return;

            if (currentLine == null)
            {
                Console.Clear();
                lineProcesses.TryDequeue(out currentLine);
                OnLineStart?.Invoke(currentLine);
            }

            // We can now add to the currently displayed screen
            linePosition = -1;
            while (waitingForUserInput == false)
            {
                Next();
                if (linePosition > currentLine.Content?.Length - 1)
                {
                    waitingForUserInput = true;
                    OnLineFinished?.Invoke(currentLine!);
                    return;
                }
                currentLine!.ReadPosAndExecute(linePosition);
                currentLetter = currentLine.Content?[linePosition];
                OnLineSubstringChange?.Invoke(currentLine);
                Thread.Sleep((int)TextRate);
            }

            void Next() => linePosition++;
        }

        public static void SetTextRate(uint rate)
        {
            TextRate = rate;
        }

        public static void MoveNextLine()
        {
            if (waitingForUserInput == false) return;
            if (lineProcesses.Count == 0)
            {
                // That was the last dialogue
                OnDialogueFinish?.Invoke(currentLine);
                Reset();
                return;
            }
            OnNextLine?.Invoke(currentLine!);
            Reset();
        }

        private static void Reset()
        {
            currentLine = null;
            currentLetter = null;
            Thread.Sleep(10);
            waitingForUserInput = false;
        }

        #region Extensions
        public static void Feed(this ref ReadOnlySpan<char> text)
        {
            if (IsInitialized == false) Initialize();
            text = currentLetter?.ToString()!;
        }
        #endregion
    }
}
