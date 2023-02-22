using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using XVNML.Core.Dialogue.Enums;
using XVNML.Core.Lexer;
using XVNML.XVNMLUtility.Tags;

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

        // Macro Data
        internal List<MacroBlockInfo> macroInvocationList;

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
            if (Content == null) return;
            //TODO: Find Macro Data and extract
            //We're going to replace each block with a control
            //character of /0 to denote a macro is in that position

            for(int i = 0; i < Content.Length; i++)
            {
                var currentCharacter = Content[i];
                if(currentCharacter == '{')
                {
                    // A new block as been established.
                    EvaluateBlock(i, out int length);

                    // Remove block from final content
                    Content = Content.Remove(i, length - 1);
                }
            }
        }

        private void EvaluateBlock(int position, out int length)
        {
            length = 0;
            if (Content == null) return;

            Tokenizer tokenizer;
            MacroBlockInfo newBlock = new MacroBlockInfo();

            var pos = 0;
            var finished = false;
            var macroCount = 0;
         

            while (finished == false)
            {
                finished = Content[position + length] == '}';
                length++;
            }
            
            tokenizer = new Tokenizer(Content.Substring(position, length), TokenizerReadState.Local, out bool conflict);
            finished = false;
            newBlock.Initialize(macroCount);
            newBlock.SetPosition(position);
            TokenType? expectingType = TokenType.Identifier;

            string? macroName = null;
            List<object> macroArgs = new List<object>();
            bool multiArgs = false;
            while (finished == false)
            {
                Next();
                SyntaxToken? currentToken = tokenizer[pos];
                if (currentToken.Type == TokenType.WhiteSpace) 
                    continue;
                if (expectingType.Value.HasFlag(currentToken?.Type) == false) return;

                switch (currentToken.Type)
                {
                    case TokenType.Identifier:
                        // If we don't have a name
                        // be sure we add that.
                        if (macroName == null)
                        {
                            macroName = currentToken.Text;

                            // We're now expecting a double colon after this.
                            // or a closed curly bracket
                            expectingType = TokenType.DoubleColon |
                                            TokenType.CloseCurlyBracket;
                            continue;
                        }

                        //Otherwise, it could be interpreted as an Enum or Flag
                        macroArgs.Add(currentToken?.Text!);

                        // Expect a comma or ) if it's multi
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                                      TokenType.Comma;
                            continue;
                        }
                        expectingType = TokenType.CloseCurlyBracket;
                        continue;

                    // If this was successful,
                    // our expecting type is either
                    // a number, a string, or an open paranthesis
                    // (which denotes a macro with multiple arguments)
                    case TokenType.DoubleColon:
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier |
                                        TokenType.OpenParentheses;
                        continue;

                    // If we encounter (, we have a macro with
                    // multiple args.
                    // Expect a number, string, or identifier
                    case TokenType.OpenParentheses:
                        multiArgs = true;
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier;
                        continue;

                    // Check if we're multiArgs
                    // If we're not, abort.
                    // Otherise, expect a number,
                    // a string, or an identifier
                    case TokenType.Comma:
                        if (multiArgs == false) return;
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier;
                        continue;

                    // The collection of multiple
                    // arguments is complete of this macro.
                    // Expect a } or a | next;
                    case TokenType.CloseParentheses:
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
                        continue;

                    // There are more macros in this block
                    // Always expect an Identifier
                    case TokenType.Line:
                        if (macroName == null) return;

                        expectingType = TokenType.Identifier;

                        newBlock!.macroCalls![macroCount].macroSymbol = macroName;
                        newBlock.macroCalls[macroCount].args = macroArgs.ToArray();
                        macroArgs.Clear();
                        macroName = null;
                        macroCount++;
                        continue;
                    
                        // We are now done with this block
                    case TokenType.CloseCurlyBracket:
                        macroInvocationList.Add(newBlock);
                        macroArgs.Clear();
                        finished = true;
                        continue;

                    case TokenType.Number:
                        macroArgs.Add(currentToken?.Text!);
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        //Otherwise, it could be interpreted as an Enum or Flag
                        expectingType = TokenType.CloseCurlyBracket;
                        continue;

                    case TokenType.String:
                        macroArgs.Add(currentToken?.Text!);
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        //Otherwise, it could be interpreted as an Enum or Flag
                        expectingType = TokenType.CloseCurlyBracket;
                        continue;

                    case TokenType.EmptyString:
                        //Otherwise, it could be interpreted as an Enum or Flag
                        macroArgs.Add(currentToken?.Text!);
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        expectingType = TokenType.CloseCurlyBracket;
                        continue;
                }
            }

            void Next() => pos++;
            SyntaxToken? Peek(int length)
            {
                return tokenizer[pos + length];
            }
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

    internal struct MacroBlockInfo
    {
        internal int blockPosition;
        internal (string macroSymbol, object[] args)[] macroCalls;
        internal void Initialize(int size)
        {
            macroCalls = new (string macroSymbol, object[] args)[size];
        }
        internal void SetPosition(int position)
        {
            blockPosition = position;
        }
    }
}