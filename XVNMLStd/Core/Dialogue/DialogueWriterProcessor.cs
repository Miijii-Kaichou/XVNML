using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Timers;
using XVNML.Core.Dialogue;

namespace XVNML.Core.Dialogue
{
    public sealed class DialogueWriterProcessor
    {
        internal static DialogueWriterProcessor Instance { get; private set; }

        public int ID { get; internal set; }
        public string? DisplayingContent => _processBuilder.ToString();
        public bool WasControlledPause { get; private set; }
        public uint ProcessRate { get; internal set; } = 60;

        public static bool IsStagnant => Instance.lineProcesses.Count == 0;

        internal ConcurrentQueue<DialogueLine> lineProcesses = new ConcurrentQueue<DialogueLine>();
        internal DialogueLine? currentLine;
        internal bool isPaused;
        internal bool doDetain;
        internal int linePosition;

        internal bool IsOnDelay => delayTimer != null;

        private  StringBuilder _processBuilder;
        private char? _currentLetter;
        private Timer? delayTimer;

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

        internal void SetProcessRate(uint rate)
        {
            ProcessRate = rate;
        }

        internal void Append(string text)
        {
            _processBuilder.Append(text);
        }

        internal void Append(char letter)
        {
            _processBuilder.Append(letter);
        }

        internal void Clear()
        {
            _processBuilder.Clear();
        }

        internal void Pause()
        {
            WasControlledPause = true;
            
        }

        internal void Unpause()
        {
            WasControlledPause = false;
            CurrentLetter = currentLine?.Content?[linePosition];
        }
        internal void Feed()
        {
            _processBuilder.Append(CurrentLetter);
        }

        internal void Wait(uint milliseconds)
        {
            delayTimer = new Timer(milliseconds);
            delayTimer.Elapsed += OnTimedEvent;
            delayTimer.AutoReset = false;
            delayTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            delayTimer = null;
            CurrentLetter = currentLine?.Content?[linePosition];
        }

        internal static DialogueWriterProcessor? Initialize(DialogueScript input, int id)
        {
            if (id < 0) return null;

            Instance = new DialogueWriterProcessor()
            {
                ID = id,
                lineProcesses = new ConcurrentQueue<DialogueLine>(),
                _processBuilder = new StringBuilder(),
                currentLine = null,
                CurrentLetter = null,
                linePosition = -1,
                isPaused = false,
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