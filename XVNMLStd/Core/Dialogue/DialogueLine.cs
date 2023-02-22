using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using XVNML.Core.Dialogue.Enums;

namespace XVNML.Core.Dialogue
{

    public sealed class DialogueLine
    {
        string? _castName;
        public string? CastName
        {
            get
            {
                return _castName;
            }
            set
            {
                _castName = value;
                GenerateCast();
            }
        }

        string? _expression;
        public string? Expression
        {
            get
            {
                return _expression;
            }
            set
            {
                _expression = value;
                GenerateExpression();
            }
        }

        string? _voice;
        public string? Voice
        {
            get
            {
                return _voice;
            }
            set
            {
                _voice = value;
                GenerateVoice();
            }
        }

        private readonly StringBuilder _ContentStringBuilder = new StringBuilder();
        public string? Content { get; private set; }
        public Dictionary<string, (int, int)> PromptContent { get; private set; } = new Dictionary<string, (int, int)>();

        public DialogueLineMode Mode { get; set; }
        internal CastMemberSignature? SignatureInfo { get; set; }

        private const int _DefaultPromptCapacity = 4;

        /// <summary>
        /// Resolves expression states on object to fully control it
        /// in code
        /// </summary>
        void GenerateExpression([CallerMemberName] string expName = "")
        {

        }

        /// <summary>
        /// Resolves voice states on object to fully control it
        /// in code
        /// </summary>
        void GenerateVoice([CallerMemberName] string voiceName = "")
        {

        }

        /// <summary>
        /// Resolves Cast Association when used in other part of XVNNML document
        /// </summary>
        /// <param name="castName"></param>
        void GenerateCast([CallerMemberName] string castName = "")
        {

        }

        internal void AppendContent(string text) => _ContentStringBuilder.Append(text);
        internal void SetNewChoice(string choice, int lineID)
        {
            PromptContent.Add(choice, (lineID, int.MaxValue));
        }
        internal void SetEndPointOnAllChoices(int lineID)
        {
            for (int i = 0; i < PromptContent.Count(); i++)
                PromptContent[PromptContent.Keys.ToArray()[i]] = (PromptContent[PromptContent.Keys.ToArray()[i]].Item1, lineID);
        }

        internal void FinalizeAndCleanBuilder()
        {
            // Trim any < and >
            Content = _ContentStringBuilder.ToString().TrimEnd('<', '>');

            // Get rid of excess white spaces
            CleanOutExcessWhiteSpaces();
            RemoveReturnCarriages();
            ExtractMacroBlocks();
        }

        private void ExtractMacroBlocks()
        {
            //TODO: Find Macro Data and extract
        }

        private void CleanOutExcessWhiteSpaces()
        {
            if (Content == null) return;
            for (int i = 0; i < Content.Length; i++)
            {
                if (Content[i] == '\n')
                {
                    int peek = 0;
                    bool cleared = false;
                    while (cleared == false)
                    {
                        peek++;
                        cleared = !(Content[i + peek] == ' ');
                    }
                    i++;
                    Content = Content.Remove(i, peek - 1);
                }
            }
        }

        private void RemoveReturnCarriages()
        {
            if (Content == null) return;
            Content = Content.Replace("\r", string.Empty).
                              Replace("\n", string.Empty);
        }
    }
}