using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public string? Response { get; internal set; }
        public bool AtEnd => lineProcessIndex > lineProcesses.Count - 1;
        public bool WasControlledPause { get; private set; }
        public uint ProcessRate { get; internal set; } = 60;
        public bool IsPaused { get; internal set; }

        public static bool IsStagnant => Instance?.lineProcesses.Count == 0;

        internal ConcurrentBag<SkripterLine> lineProcesses = new ConcurrentBag<SkripterLine>();
        internal SkripterLine? currentLine;
        internal int lineProcessIndex = -1;
        internal bool doDetain;
        internal int linePosition;

        private int previousLinePosition;
        internal bool IsOnDelay => delayTimer != null;

        private StringBuilder _processBuilder = new StringBuilder();
        private char? _currentLetter;
        private Timer? delayTimer;

        internal object processLock = new object();
        internal bool HasChanged => previousLinePosition != linePosition;
        internal bool inPrompt;

        private CastInfo? _currentCastInfo;
        private Stack<int> _returnPointStack = new Stack<int>();
        private bool _lastProcessWasClosing;

        private SceneInfo? _currentSceneInfo = null;

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
                if (_currentCastInfo == null) return;

                if (_currentCastInfo!.Value.name?.Equals(previous?.name) == false)
                {
                    DialogueWriter.OnCastChange?[ID]?.Invoke(this);
                }

                if (_currentCastInfo!.Value.expression?.Equals(previous?.expression) == false)
                {
                    DialogueWriter.OnCastExpressionChange?[ID]?.Invoke(this);
                }

                if (_currentCastInfo!.Value.voice?.Equals(previous?.voice) == false)
                {
                    DialogueWriter.OnCastVoiceChange?[ID]?.Invoke(this);
                }
            }
        }

        // Scene Data
        public SceneInfo? CurrentSceneInfo
        {
            get
            {
                return _currentSceneInfo;
            }
            set
            {
                var previous = _currentSceneInfo;

                _currentSceneInfo = value;

                if (_currentSceneInfo == null) return;
                if (previous?.name == _currentSceneInfo?.name) return;

                if (_currentSceneInfo!.Value.name?.Equals(previous?.name) == false)
                {
                    DialogueWriter.OnSceneChange?[ID]?.Invoke(this);
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
                lineProcesses = new ConcurrentBag<SkripterLine>(),
                _processBuilder = new StringBuilder(),
                currentLine = null,
                CurrentLetter = null,
                linePosition = -1,
                IsPaused = false,
                doDetain = false
            };

            foreach (SkripterLine line in input.Lines.Reverse())
            {
                Instance.lineProcesses.Add(line);
            }
            return Instance;
        }

        public Dictionary<string, (int, int)>? FetchPrompts()
        {
            return currentLine?.PromptContent;
        }

        public void JumpToStartingLineFromResponse(string response)
        {
            if (currentLine == null) return;
            inPrompt = false;
            var prompt = currentLine.PromptContent[response];
            lineProcessIndex = prompt.sp - 1;
            Response = response;
            if (_returnPointStack.Count != 0 && _returnPointStack.Peek() == prompt.rp) return;
            _returnPointStack.Push(prompt.rp);
        }

        public void JumpToReturningLineFromResponse()
        {
            if (_returnPointStack.Count == 0) return;
            var index = _returnPointStack.Pop();
            if (lineProcessIndex == index + 1)
            {
                JumpToReturningLineFromResponse();
                return;
            }
            lineProcessIndex = index;
        }

        internal void NextProcess() => lineProcessIndex++;

        internal void UpdateProcess()
        {
            if (_returnPointStack.Count != 0 && _lastProcessWasClosing)
            {
                JumpToReturningLineFromResponse();
                if (AtEnd) return;
                currentLine = lineProcesses.ElementAt(lineProcessIndex);
                _lastProcessWasClosing = currentLine.data.isClosingLine;
                return;
            }
            NextProcess();
            if (AtEnd) return;
            currentLine = lineProcesses.ElementAt(lineProcessIndex);
            _lastProcessWasClosing = currentLine.data.isClosingLine;
        }
    }
}