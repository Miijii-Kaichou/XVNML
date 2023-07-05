using Newtonsoft.Json;
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
        [JsonProperty] internal int lineIndex;
        [JsonProperty] internal int returnPoint;
        [JsonProperty] internal bool isPartOfResponse;
        [JsonProperty] internal string? fromResponse;
        [JsonProperty] internal SkripterLine? parentLine;
        [JsonProperty] internal bool isClosingLine;
        [JsonProperty] internal string? responseString;

        public DialogueLineMode Mode { get; set; }
    }

    public sealed class SkripterLine
    {
        
        private static SkripterLine? Instance;

        internal string? lastAddedResponse;
        [JsonProperty] internal LineDataInfo data;
        internal Stack<string> LastAddedResponseStack = new Stack<string>();

        private readonly StringBuilder _ContentStringBuilder = new StringBuilder();
        [JsonProperty] public string? Content { get; private set; }
        [JsonProperty] public Dictionary<string, (int sp, int rp)> PromptContent { get; private set; } = new Dictionary<string, (int, int)>();
        [JsonProperty] public DialogueLineMode Mode => data.Mode;

        // Unique Tag
        [JsonProperty] internal string? TaggedAs = string.Empty;

        // Scene Data
        [JsonProperty] internal SceneInfo? SceneLoadInfo { get; set; }

        // Cast Data
        [JsonProperty] internal CastMemberSignature? SignatureInfo { get; set; }
        [JsonProperty] internal CastInfo? InitialCastInfo { get; set; }

        // Macro Data
        [JsonProperty] internal List<MacroBlockInfo> macroInvocationList = new List<MacroBlockInfo>();
        internal Action? onReturnPointCorrection;
        private Tokenizer _sharedTokenizer;

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
                var sp = PromptContent[response].sp;
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
            _sharedTokenizer = new Tokenizer(stringBuilder.ToString(), TokenizerReadState.Local);

            bool conflictResult = _sharedTokenizer.GetConflictState();

            const string ExpressionCode = "E::";
            const string VoiceCode = "V::";

            int _position = -1;

            string? expression = null;
            string? voice = null;

            CastEvaluationMode mode = CastEvaluationMode.Expression;

            for (int i = 0; i < _sharedTokenizer.Length; i++)
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
                if (_sharedTokenizer == null) return null;
                var index = _position + offset;

                var filteredTokens = _sharedTokenizer.definedTokens.Skip(index)
                    .Where(token =>
                        (includeSpaces || token?.Type != TokenType.WhiteSpace) &&
                        token?.Type != TokenType.SingleLineComment &&
                        token?.Type != TokenType.MultilineComment);

                return filteredTokens.FirstOrDefault();
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

            MacroBlockInfo newBlock = new MacroBlockInfo();

            var pos = 0;
            var finished = false;
            var macroCount = 0;

            while (!finished)
            {
                finished = Content[position + length] == '}';
                length++;
            }
            var blockString = Content[position..(position+length)];
            var macrosTotal = blockString.Split('|').Length;

            _sharedTokenizer = new Tokenizer(blockString, TokenizerReadState.Local);
            bool conflict = _sharedTokenizer.GetConflictState();

            if (conflict) return;

            finished = false;
            newBlock.Initialize(macrosTotal);
            newBlock.SetPosition(position);
            TokenType? expectingType = TokenType.Identifier;

            string? macroName = null;
            List<(object value, Type type)> macroArgs = new List<(object, Type)>();
            bool multiArgs = false;
            while (!finished)
            {
                Next();
                SyntaxToken? currentToken = _sharedTokenizer[pos];

                if (currentToken?.Type == TokenType.WhiteSpace)
                    continue;

                if (!expectingType.Value.HasFlag(currentToken?.Type))
                    return;

                switch (currentToken?.Type)
                {
                    case TokenType.Identifier:
                        if (macroName == null)
                        {
                            macroName = currentToken.Text;
                            expectingType = TokenType.DoubleColon |
                                            TokenType.CloseCurlyBracket |
                                            TokenType.Line;
                            continue;
                        }

                        var identifierType = typeof(object);

                        if (currentToken.Text?.ToLower() == "true" ||
                            currentToken.Text?.ToLower() == "false")
                        {
                            identifierType = typeof(bool);
                        }

                        macroArgs.Add((currentToken?.Text!, identifierType));

                        if (multiArgs)
                        {
                            expectingType = TokenType.CloseParentheses |
                                            TokenType.Comma;
                            continue;
                        }
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
                        continue;

                    case TokenType.DoubleColon:
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier |
                                        TokenType.OpenParentheses;
                        continue;

                    case TokenType.OpenParentheses:
                        multiArgs = true;
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier;
                        continue;

                    case TokenType.Comma:
                        if (!multiArgs)
                            return;
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier;
                        continue;

                    case TokenType.Line:
                        if (macroName == null)
                            return;

                        expectingType = TokenType.Identifier;
                        PopulateBlock(newBlock, macroCount, macroName, macroArgs);
                        macroArgs.Clear();
                        macroName = null;
                        multiArgs = false;
                        macroCount++;
                        continue;

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
                        expectingType = TokenType.CloseCurlyBracket |
                                        TokenType.Line;
                        continue;

                    case TokenType.EmptyString:
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
                throw new ArgumentNullException(nameof(macroSymbol), "There was no macro symbol present.");
            }

            if (!DefinedMacrosCollection.ValidMacros?.ContainsKey(macroSymbol) == true)
            {
                throw new InvalidMacroException($"The macro \"{macroSymbol}\" is undefined.", macroSymbol, Instance!);
            }

            newBlock!.macroCalls![macroCount].macroSymbol = macroSymbol;
            newBlock.macroCalls[macroCount].args = macroArgs.ToArray();
        }

        private void CleanOutExcessWhiteSpaces()
        {
            if (Content == null) return;
            int i = 0;
            while (i < Content.Length)
            {
                if (Content[i] == '\n')
                {
                    int peek = 0;
                    bool cleared = false;
                    while (!cleared)
                    {
                        peek++;
                        cleared = !(Content[i + peek] == ' ');
                    }
                    i++;
                    Content = Content.Remove(i, peek - 1);
                }
                else
                {
                    i++;
                }
            }
        }

        private void RemoveReturnCarriages()
        {
            if (Content == null) return;
            Content = Content.Replace("\r", string.Empty)
                             .Replace("\n", string.Empty)
                             .Replace("\t", string.Empty);
        }

        private void EvaluateNumericType(SyntaxToken? token, out Type? resultType)
        {
            int length = token!.Text!.Length;
            var text = token!.Text;
            var character = text[length - 1];
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