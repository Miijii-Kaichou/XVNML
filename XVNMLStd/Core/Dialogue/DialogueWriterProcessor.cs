using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using XVNML.Core.Dialogue.Structs;
using XVNML.Core.Macros;
using XVNML.Utilities.Dialogue;
using XVNML.Utilities.Macros;

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
        public bool AtEnd => lineIndex > lineProcesses.Count - 1;
        public bool WasControlledPause { get; private set; }
        public uint ProcessRate { get; internal set; } = 60;
        public bool IsPaused { get; internal set; }
        public bool IsPass { get; internal set; } = false;

        public static bool IsStagnant => Instance?.lineProcesses.Count == 0;

        internal ConcurrentBag<SkripterLine> lineProcesses = new ConcurrentBag<SkripterLine>();
        internal SkripterLine? currentLine;
        internal int lineIndex = -1;
        internal int cursorIndex;

        private int previousLinePosition;
        internal bool IsOnDelay => delayTimer != null;

        private StringBuilder _processBuilder = new StringBuilder();
        private char? _currentLetter;
        private Timer? delayTimer;

        internal object processLock = new object();
        internal bool HasChanged => previousLinePosition != cursorIndex;
        internal bool inPrompt;

        private CastInfo? _currentCastInfo;
        private bool _lastProcessWasClosing;

        private SceneInfo? _currentSceneInfo = null;
        private int _jumpIndexValue = -1;

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
            DialogueWriter.OnLineSubstringChange?[ID]?.Invoke(this);
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
            previousLinePosition = lineIndex;
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
                cursorIndex = -1,
                IsPaused = false,
                processLock = new object(),
            };

            var reversedList = input.Lines?.ToArray().Reverse();

            for (int i = 0; i < reversedList.Count(); i++)
            {
                SkripterLine line = reversedList.ElementAt(i);

                if (line.data.parentLine != null && line.data.isClosingLine)
                {
                    line.data.returnPoint = line.data.parentLine.PromptContent[line.data.responseString!].rp;
                }

                Instance.lineProcesses.Add(line);
            }

            return Instance;
        }

        public Dictionary<string, (int, int)>? FetchPrompts()
        {
            return currentLine?.PromptContent;
        }

        public void JumpTo(string lineTagName)
        {
            _jumpIndexValue = Instance!.lineProcesses.Where(sl => sl.Name == lineTagName).Single().data.lineIndex;
        }

        public void JumpTo(int index)
        {
            _jumpIndexValue = Instance!.lineProcesses.Where(sl => sl.data.lineIndex == index).Single().data.lineIndex;
        }

        public void LeadTo(string lineTagName)
        {
            // Increment through line processes from current index
            // until you find the next tagged Tag Name
            if (Instance!.lineProcesses.ElementAt(_jumpIndexValue++).Name!.Equals(lineTagName)) return;
            LeadTo(lineTagName);
        }

        public void LeadTo(int index)
        {
            // Increment [index] amount of times
            _jumpIndexValue++;
            if (index > 0) index--;
            LeadTo(index);
        }

        public void JumpToStartingLineFromResponse(string response)
        {
            if (currentLine == null) return;
            inPrompt = false;
            var prompt = currentLine.PromptContent[response];
            lineIndex = prompt.sp - 1;
            Response = response;
        }

        public void JumpToReturningLineFromResponse()
        {
            var recentLine = lineProcesses.ElementAt(lineIndex);
            var index = -1;

            if (recentLine != null && recentLine.data.isClosingLine) index = recentLine.data.returnPoint;

            if (lineIndex == index + 1)
            {
                JumpToReturningLineFromResponse();
                return;
            }
            lineIndex = index;
        }

        internal void NextProcess() => lineIndex++;

        internal void UpdateProcess()
        {
            if (_jumpIndexValue != -1)
            {
                lineIndex = _jumpIndexValue;
                _jumpIndexValue = -1;
                if (AtEnd) return;

                UpdateLine();
                return;
            }

            if (_lastProcessWasClosing && lineIndex != lineProcesses.Count - 1)
            {
                JumpToReturningLineFromResponse();
                if (AtEnd) return;

                UpdateLine();
                return;
            }
            NextProcess();
            if (AtEnd) return;

            UpdateLine();
        }

        private void UpdateLine()
        {
            currentLine = lineProcesses.ElementAt(lineIndex);
            _lastProcessWasClosing = currentLine.data.isClosingLine;
        }

        internal void ResetPass()
        {
            IsPass = false;
        }

        internal void AllowPass()
        {
            if (currentLine?.data.Mode == Enums.DialogueLineMode.Prompt) return;
            IsPass = true;
        }

        internal void Wipe()
        {
            _processBuilder.Clear();
            IsPass = false;
            currentLine = null;
            cursorIndex = -1;
            lineProcesses.Clear();
            CurrentLetter = null;
            IsPaused = false;
        }
    }
}