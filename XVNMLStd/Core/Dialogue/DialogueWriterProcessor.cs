using System;
using System.Collections.Concurrent;
using System.Text;
using XVNML.Core.Dialogue;

namespace XVNML.Core.Dialogue
{
    public sealed class DialogueWriterProcessor
    {
        internal static DialogueWriterProcessor Instance { get; private set; }

        public int ProcessID { get; internal set; }
        public string? DisplayingContent => processBuilder.ToString();

        public static bool IsStagnant => Instance.lineProcesses.Count == 0;

        internal ConcurrentQueue<DialogueLine> lineProcesses;
        internal DialogueLine? currentLine;
        internal bool waitingForUserInput;
        internal bool doDetain;
        internal int linePosition;
        internal uint processRate = 60;
        internal StringBuilder processBuilder;

        private char? _currentLetter;
        internal char? CurrentLetter
        {
            get
            {
                return _currentLetter;
            }
            set {
                _currentLetter = value;
                Feed();
            }
        }

        public void SetProcessRate(uint rate)
        {
            processRate = rate;
        }

        public void Append(string text)
        {
            processBuilder.Append(text);
        }

        public void Append(char letter)
        {
            processBuilder.Append(letter);
        }

        internal void Feed()
        {
            processBuilder.Append(CurrentLetter);
        }

        internal static DialogueWriterProcessor? Initialize(DialogueScript input, int id)
        {
            if (id < 0) return null;

            Instance = new DialogueWriterProcessor()
            {
                ProcessID = id,
                lineProcesses = new ConcurrentQueue<DialogueLine>(),
                processBuilder = new StringBuilder(),
                currentLine = null,
                CurrentLetter = null,
                linePosition = -1,
                waitingForUserInput = false,
                doDetain = false
            };

            foreach(DialogueLine line in input.Lines)
            {
                Instance.lineProcesses.Enqueue(line);
            }

            return Instance;
        }
    } 
}