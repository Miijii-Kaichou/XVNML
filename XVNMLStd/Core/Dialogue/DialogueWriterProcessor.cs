using System;
using System.Collections.Concurrent;
using System.Text;
using System.Timers;
using XVNML.Core.Dialogue.Structs;
using XVNML.Core.Macros;
using XVNML.Utility.Diagnostics;
using XVNML.Utility.Dialogue;
using XVNML.Utility.Macros;

namespace XVNML.Core.Dialogue
{
    public sealed class DialogueWriterProcessor
    {
        internal static DialogueWriterProcessor? Instance { get; private set; }

        public int ID { get; internal set; }
        public string? DisplayingContent
        {
            get
            {
                lock (processLock)
                {
                    return _processBuilder.ToString();
                }
            }
        }

        public bool WasControlledPause { get; private set; }
        public uint ProcessRate { get; internal set; } = 60;
        public bool IsPaused { get; internal set; }

        public static bool IsStagnant => Instance?.lineProcesses.Count == 0;

        internal ConcurrentQueue<DialogueLine> lineProcesses = new ConcurrentQueue<DialogueLine>();
        internal DialogueLine? currentLine;
        internal bool doDetain;
        internal int linePosition;
        private int previousLinePosition;

        internal bool IsOnDelay => delayTimer != null;

        private StringBuilder _processBuilder = new StringBuilder();
        private char? _currentLetter;
        private Timer? delayTimer;

        internal object processLock = new object();
        internal bool HasChanged => previousLinePosition != linePosition;

        private CastInfo? _currentCastInfo;
        //Cast Data
        public CastInfo? CurrentCastInfo
        {
            get
            {
                return _currentCastInfo;
            }
            set
            {
                var previous = _currentCastInfo;

                _currentCastInfo = value;

                if (previous == null) return;

                if (_currentCastInfo!.Value.name?.Equals(previous!.Value.name) == false)
                {
                    DialogueWriter.OnCastChange?[ID]?.Invoke(this);
                }

                if (_currentCastInfo!.Value.expression?.Equals(previous!.Value.expression) == false)
                {
                    DialogueWriter.OnCastExpressionChange?[ID]?.Invoke(this);
                }

                if (_currentCastInfo!.Value.voice?.Equals(previous!.Value.voice) == false)
                {
                    DialogueWriter.OnCastVoiceChange?[ID]?.Invoke(this);
                }
            }
        }

        internal char? CurrentLetter
        {
            get
            {
                return _currentLetter;
            }
            set
            {
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
            lock (processLock)
            {
                _processBuilder.Append(text);
                DialogueWriter.OnLineSubstringChange?[ID]?.Invoke(this);
            }
        }

        internal void Append(char letter)
        {
            lock (processLock)
            {
                _processBuilder.Append(letter);
                DialogueWriter.OnLineSubstringChange?[ID]?.Invoke(this);
            }
        }

        internal void Clear()
        {
            _processBuilder.Clear();
        }

        internal void Pause()
        {
            MacroInvoker.Block(this);
            WasControlledPause = true;
            DialogueWriter.WaitingForUnpauseCue![ID] = WasControlledPause;
            DialogueWriter.OnLinePause?[ID]?.Invoke(this);
        }

        internal void Unpause()
        {
            MacroInvoker.UnBlock(this);
            WasControlledPause = false;
            DialogueWriter.WaitingForUnpauseCue![ID] = WasControlledPause;
        }

        internal void Feed()
        {
            lock (processLock)
            {
                _processBuilder.Append(CurrentLetter);
            }
        }

        internal void Wait(uint milliseconds)
        {
            MacroInvoker.Block(this);
            WasControlledPause = true;
            delayTimer = new Timer(milliseconds);
            delayTimer.Elapsed += OnTimedEvent;
            delayTimer.AutoReset = false;
            delayTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            MacroInvoker.UnBlock(this);
            WasControlledPause = false;
            delayTimer = null;
        }

        internal void ChangeCastVoice(MacroCallInfo info, string voiceName)
        {
            CurrentCastInfo = new CastInfo()
            {
                name = _currentCastInfo!.Value.name,
                expression = _currentCastInfo!.Value.expression,
                voice = voiceName
            };

            XVNMLLogger.Log(CurrentCastInfo.ToString(), this);
        }

        internal void ChangeCastExpression(MacroCallInfo info, string expressionName)
        {
            CurrentCastInfo = new CastInfo()
            {
                name = _currentCastInfo!.Value.name,
                expression = expressionName,
                voice = _currentCastInfo!.Value.voice,
            };
        }

        internal void UpdatePrevious()
        {
            previousLinePosition = linePosition;
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
                IsPaused = false,
                doDetain = false
            };

            foreach (DialogueLine line in input.Lines)
            {
                Instance.lineProcesses.Enqueue(line);
            }

            return Instance;
        }

    }
}