using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XVNML.Core.Dialogue.Enums;
using XVNML.Core.Dialogue.Structs;
using XVNML.Core.Lexer;
using XVNML.Core.Macros;
using XVNML.Utility.Diagnostics;
using XVNML.Utility.Macros;

namespace XVNML.Core.Dialogue
{
    internal struct LineDataInfo
    {
        internal int lineIndex;
        internal int returnPoint;
        internal bool isPartOfResponse;
        internal string? fromResponse;
        internal SkripterLine? parentLine;
        internal bool isClosingLine;
        internal string? responseString;

        public DialogueLineMode Mode { get; set; }
    }

    public sealed class SkripterLine
    {
        private static SkripterLine? Instance;

        internal string? lastAddedResponse;
        internal LineDataInfo data;
        internal Stack<string> LastAddedResponseStack = new Stack<string>();

        private readonly StringBuilder _ContentStringBuilder = new StringBuilder();
        public string? Content { get; private set; }
        public Dictionary<string, (int sp, int rp)> PromptContent { get; private set; } = new Dictionary<string, (int, int)>();
        public DialogueLineMode Mode => data.Mode;

        // Unique Tag
        internal string? TaggedAs = string.Empty;

        // Scene Data
        internal SceneInfo? SceneLoadInfo { get; set; }

        // Cast Data
        internal CastMemberSignature? SignatureInfo { get; set; }
        internal CastInfo? InitialCastInfo { get; set; }

        // Macro Data
        internal List<MacroBlockInfo> macroInvocationList = new List<MacroBlockInfo>();
        internal Action? onReturnPointCorrection;

        internal void ReadPosAndExecute(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                macroInvocationList
                .Where(macro => macro.blockPosition.Equals(process.cursorIndex))
                .ToList()
                .ForEach(macro => macro.Call(new MacroCallInfo() { process = process, callIndex = process.cursorIndex }));
            }
        }

        internal void AppendContent(string text) => _ContentStringBuilder.Append(text);

        internal void SetNewChoice(string choice, int lineID)
        {
            if (PromptContent.ContainsKey(choice)) return;
            PromptContent.Add(choice, (lineID, -1));
            LastAddedResponseStack.Push(choice);
        }

        internal void SetReturnPointOnAllChoices(int index)
        {
            if (LastAddedResponseStack.Count == 0) return;
            while (LastAddedResponseStack.Count > 0)
            {
                var response = LastAddedResponseStack.Pop();
                var sp = PromptContent[response].Item1;
                PromptContent[response] = (sp, index);
            }
            data.isPartOfResponse = true;
        }

        internal void CorrectReturnPointOnAllChoices(int index)
        {
            var temp = PromptContent;
            PromptContent = new Dictionary<string, (int sp, int rp)>();
            foreach (var kvp in temp)
            {
                var response = kvp.Key;
                PromptContent.Add(response, (temp[response].sp, index));
            }
        }

        internal void MarkAsClosing() => data.isClosingLine = true;


        internal void FinalizeAndCleanBuilder()
        {
            // Trim any < and >
            Content = _ContentStringBuilder
                .ToString()
                .TrimEnd('<')
                .Replace(">>", string.Empty);

            // Replacing .Replace(">", "{pause}"); with
            // with a method that checks if it has >ExpName>VoiceName or anything like that
            // We'll convert it to BlockNotation just like how we did for the pause
            for (int i = 0; i < Content.Length; i++)
            {
                var character = Content[i];

                if (character == '>') ParsePauseControlCharacter(i);
            }

            // Get rid of excess white spaces
            CleanOutExcessWhiteSpaces();
            RemoveReturnCarriages();
            ExtractMacroBlocks();
        }

        private void ParsePauseControlCharacter(int startIndex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = startIndex; i < Content?.Length; i++)
            {
                var character = Content[i];

                if (char.IsWhiteSpace(character))
                {
                    ConvertToCallBlock(stringBuilder);
                    break;
                }
                stringBuilder.Append(character);
            }
        }

        private void ConvertToCallBlock(StringBuilder stringBuilder)
        {
            // Use this as reference
            // @Me Hi!>Smile>000 {clr}How's it going?<<

            Tokenizer tokenizer = new Tokenizer(stringBuilder.ToString(), TokenizerReadState.Local, out bool conflictResult);

            const string ExpressionCode = "E::";
            const string VoiceCode = "V::";

            int _position = -1;

            string? expression = null;
            string? voice = null;

            CastEvaluationMode mode = CastEvaluationMode.Expression;

            for (int i = 0; i < tokenizer.Length; i++)
            {
                if (conflictResult) return;

                Next();

                SyntaxToken? token = Peek(0, true);

                if (token?.Type == TokenType.CloseBracket)
                {
                    Next();
                    token = Peek(0, true);
                    if (token?.Type == TokenType.DoubleColon)
                    {
                        continue;
                    }

                    var isQualifiedToken = token?.Type == TokenType.Identifier ||
                    token?.Type == TokenType.Number;


                    // We have 2 outputs for this one: Expression or Voice
                    // Check for E:: or V::
                    if (isQualifiedToken &&
                        (token?.Text == ExpressionCode[0].ToString() ||
                        token?.Text == VoiceCode[0].ToString()))
                    {
                        mode = token?.Text == ExpressionCode[0].ToString() ? CastEvaluationMode.Expression : CastEvaluationMode.Voice;

                        //Otherwise, see where we are. If one of them is null,
                        //check if the other one isn't. If it isn't, we have to fill
                        //information for the one that doesnt' have anything.
                        if (expression == null && voice != null && isQualifiedToken)
                        {
                            expression = voice;
                            Next();
                            continue;
                        }

                        if (voice == null && expression != null && isQualifiedToken)
                        {
                            voice = expression;
                            Next();
                            continue;
                        }

                        Next();
                        continue;
                    }
                }

                if (token?.Type == TokenType.EOF) break;

                //Otherwise, check the phase
                if (mode == CastEvaluationMode.Expression)
                {
                    expression = token?.Text;
                    mode = CastEvaluationMode.Voice;
                    continue;
                }

                if (mode == CastEvaluationMode.Voice)
                {
                    voice = token?.Text;
                    mode = CastEvaluationMode.Expression;
                    continue;
                }
            }

            StringBuilder blockStringBuilder = new StringBuilder("{pause");

            if (expression != null)
            {
                blockStringBuilder.Append("|expression::");
                blockStringBuilder.Append("\"");
                blockStringBuilder.Append(expression.ToString());
                blockStringBuilder.Append("\"");
            }

            if (voice != null)
            {
                blockStringBuilder.Append("|voice::");
                blockStringBuilder.Append("\"");
                blockStringBuilder.Append(voice.ToString());
                blockStringBuilder.Append("\"");
            }

            blockStringBuilder.Append("}");

            Content = Content?.Replace(stringBuilder.ToString(), blockStringBuilder.ToString());

            SyntaxToken? Peek(int offset, bool includeSpaces = false)
            {
                if (tokenizer == null) return null;
                try
                {
                    var token = tokenizer[_position + offset];

                    while (true)
                    {
                        token = tokenizer[_position + offset];

                        if ((token?.Type == TokenType.WhiteSpace && includeSpaces == false) ||
                            token?.Type == TokenType.SingleLineComment ||
                            token?.Type == TokenType.MultilineComment)
                        {
                            _position++;
                            continue;
                        }

                        return token;
                    }

                }
                catch
                {
                    return tokenizer[tokenizer.Length];
                }
            }

            SyntaxToken? Next()
            {
                _position++;
                return Peek(0, true);
            }
        }

        private void ExtractMacroBlocks()
        {
            if (Content == null) return;
            //TODO: Find Macro Data and extract
            //We're going to replace each block with a control
            //character of /0 to denote a macro is in that position

            for (int i = 0; i < Content.Length; i++)
            {
                var currentCharacter = Content[i];
                if (currentCharacter == '{')
                {
                    // A new block as been established.
                    EvaluateBlock(i, out int length);

                    // Remove block from final content
                    Content = Content.Remove(i, length);
                    i--;
                }
            }
        }

        private void EvaluateBlock(int position, out int length)
        {
            length = 0;
            if (Content == null) return;

            Instance = this;

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
            var blockString = Content.Substring(position, length);
            var macrosTotal = blockString.Split('|').Length;

            tokenizer = new Tokenizer(blockString, TokenizerReadState.Local, out bool conflict);
            if (conflict) return;
            finished = false;
            newBlock.Initialize(macrosTotal);
            newBlock.SetPosition(position);
            TokenType? expectingType = TokenType.Identifier;

            string? macroName = null;
            List<(object, Type)> macroArgs = new List<(object, Type)>();
            bool multiArgs = false;
            while (finished == false)
            {
                macroInvocationList ??= new List<MacroBlockInfo>();

                Next();
                SyntaxToken? currentToken = tokenizer[pos];

                if (currentToken?.Type == TokenType.WhiteSpace)
                    continue;

                if (expectingType.Value.HasFlag(currentToken?.Type) == false) return;

                switch (currentToken?.Type)
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
                                            TokenType.CloseCurlyBracket |
                                            TokenType.Line;
                            continue;
                        }

                        var identifierType = typeof(object);

                        //Otherwise, it could be interpreted as an Enum or Flag
                        if (currentToken.Text?.ToLower() == "true" ||
                            currentToken.Text?.ToLower() == "false")
                        {
                            identifierType = typeof(bool);
                        }

                        macroArgs.Add((currentToken?.Text!, identifierType));

                        // Expect a comma or ) if it's multi
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                                      TokenType.Comma;
                            continue;
                        }
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
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
                        PopulateBlock(newBlock, macroCount, macroName, macroArgs);
                        macroArgs.Clear();
                        macroName = null;
                        multiArgs = false;
                        macroCount++;
                        continue;

                    // We are now done with this block
                    case TokenType.CloseCurlyBracket:
                        PopulateBlock(newBlock, macroCount, macroName, macroArgs);
                        macroInvocationList.Add(newBlock);
                        macroArgs.Clear();
                        macroName = null;
                        finished = true;
                        multiArgs = false;
                        continue;

                    case TokenType.Number:
                        EvaluateNumericType(currentToken, out Type? type);
                        macroArgs.Add((currentToken?.Value, type!)!);
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        //Otherwise, it could be interpreted as an Enum or Flag
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
                        continue;

                    case TokenType.String:
                        macroArgs.Add((currentToken?.Text!, typeof(string)));
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        //Otherwise, it could be interpreted as an Enum or Flag
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
                        continue;

                    case TokenType.EmptyString:
                        //Otherwise, it could be interpreted as an Enum or Flag
                        macroArgs.Add((currentToken?.Text!, typeof(string)));
                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
                        continue;
                    default:
                        break;
                }
            }

            void Next() => pos++;
        }

        private static void PopulateBlock(MacroBlockInfo newBlock, int macroCount, string? macroSymbol, List<(object, Type)> macroArgs)
        {
            if (macroSymbol == null)
            {
                throw new ArgumentNullException(macroSymbol, "There was no macro symbol present.");
            }

            if (DefinedMacrosCollection.ValidMacros?.ContainsKey(macroSymbol) == false)
            {  
                throw new InvalidMacroException($"The macro \"{macroSymbol}\" is undefined.", macroSymbol, Instance!);
            }

            newBlock!.macroCalls![macroCount].macroSymbol = macroSymbol;
            newBlock.macroCalls[macroCount].args = macroArgs.ToArray();
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
                              Replace("\n", string.Empty).
                              Replace("\t", string.Empty);
        }

        private void EvaluateNumericType(SyntaxToken? token, out Type? resultType)
        {
            int length = token!.Text!.Length;
            var text = token!.Text;
            var character = text.Last();
            if (char.IsLetter(character)) text = text.Remove(length - 1, 1);

            if (char.ToUpper(character) == 'F' || text.Contains('.'))
            {
                resultType = typeof(float);
                return;
            }

            if (char.ToUpper(character) == 'D' || text.Contains('.'))
            {
                resultType = typeof(double);
                return;
            }

            if (char.ToUpper(character) == 'L')
            {
                resultType = typeof(long);
                return;
            }

            if (char.ToUpper(character) == 'I')
            {
                resultType = typeof(int);
                return;
            }


            character = text[0];

            if (character == '-')
            {
                resultType = typeof(int);
                return;
            }

            resultType = typeof(uint);
        }

        internal void SetLineTag(string? identifier)
        {
            TaggedAs = identifier;
        }
    }
}