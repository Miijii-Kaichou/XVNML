using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XVNML.Core.Dialogue.Enums;
using XVNML.Core.Dialogue.Structs;
using XVNML.Core.Lexer;
using XVNML.Core.Macros;
using XVNML.Core.Enums;
using XVNML.Utilities.Macros;
using XVNML.Core.Extensions;

using static XVNML.CharacterConstants;
using static XVNML.StringConstants;

namespace XVNML.Core.Dialogue
{
    public sealed class SkriptrLine
    {
        internal string? lastAddedResponse;
        internal Stack<string> LastAddedResponseStack = new Stack<string>();
        internal Action? onReturnPointCorrection;

        private SyntaxToken[]? _tokenCache;

        private static SkriptrLine? Instance;
        private readonly StringBuilder _ContentStringBuilder = new StringBuilder();

        private string? _content;

        [JsonProperty]
        public string? Content
        {
            get
            {
                return _content;
            }
            private set
            {
                _content = value;
            }
        }

        private bool _modified = false;

        [JsonProperty]
        public bool Modified
        {
            get
            {
                return _modified;
            }
            private set
            {
                _modified = value;
            }
        }
        
        [JsonProperty] public Dictionary<string, (int sp, int rp)> PromptContent { get; private set; } = new Dictionary<string, (int, int)>();
        [JsonProperty] public DialogueLineMode Mode => data.Mode;

        [JsonProperty] internal LineDataInfo data;
        [JsonProperty] internal string? Name = string.Empty;
        [JsonProperty] internal SceneInfo? SceneLoadInfo { get; set; }
        [JsonProperty] internal CastMemberSignature? SignatureInfo { get; set; }
        [JsonProperty] internal CastInfo? InitialCastInfo { get; set; }
        [JsonProperty] internal List<MacroBlockInfo> macroInvocationList = new List<MacroBlockInfo>();

        // Simply to save the state before the list was modified/changed from certain macro calls.
        private List<MacroBlockInfo>? _cachedMIL = new List<MacroBlockInfo>();
        private string? _originalContent;
        private List<MacroBlockInfo> _originalMacroList;

        internal void ReadPosAndExecute(DialogueWriterProcessor process, string rootScope = "")
        {
            lock (process.processLock)
            {
                macroInvocationList
                .Where(macro => (macro.blockPosition).Equals(process.cursorIndex))
                .ToList()
                .ForEach(macro => macro.Call(new MacroCallInfo() { process = process, callIndex = process.cursorIndex, callScope = rootScope }));
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


        internal void FinalizeAndCleanBuilder(SyntaxToken?[] tokenCache)
        {
            _tokenCache = tokenCache!;

            // Trim any < and >
            Content = _ContentStringBuilder
                .ToString()
                .TrimEnd(LessThanCharacter)
                .Replace(DoubleGreaterThanString, string.Empty);

            for (int i = 0; i < _tokenCache.Length; i++)
            {
                var token = tokenCache[i];

                if (token!.Type != TokenType.CloseBracket) continue;
                ParsePauseControlCharacter(i);
            }

            _tokenCache = Tokenizer.Tokenize(Content, TokenizerReadState.Local, true)?.ToArray()!;

            // Get rid of excess white spaces
            CleanOutExcessWhiteSpaces();
            RemoveReturnCarriages();
            ExtractMacroBlocks();
        }

        private void ParsePauseControlCharacter(int startIndex)
        {
            List<SyntaxToken?> tokenList = new List<SyntaxToken?>(128);
            for (int i = startIndex; i < _tokenCache?.Length; i++)
            {
                var token = _tokenCache[i];
                var tokenType = token.Type;

                if (tokenType == TokenType.WhiteSpace)
                {
                    ConvertToCallBlock(tokenList);
                    break;
                }
                tokenList.Add(token);
            }
        }

        private void ConvertToCallBlock(List<SyntaxToken?> tokenList)
        {
            const string ExpressionCode = ExpressionPrefixString;
            const string VoiceCode = VoicePrefixString;

            int _position = -1;

            string? expression = null;
            string? voice = null;

            var tokens = tokenList;

            int count = tokenList.Count;

            CastEvaluationMode mode = CastEvaluationMode.Expression;

            StringBuilder sb = new StringBuilder();
            List<SyntaxToken> pauseSyntaxTokens = new List<SyntaxToken>();

            for (int i = 0; i < count; i++)
            {
                Next();

                SyntaxToken? token = Peek(0, true);

                sb.Append(token?.Text);
                pauseSyntaxTokens.Add(token);

                if (token?.Type == TokenType.CloseBracket)
                {
                    Next();

                    token = Peek(0, true);
                    sb.Append(token.Text);
                    pauseSyntaxTokens.Add(token);

                    if (token?.Type == TokenType.DoubleColon)
                    {
                        continue;
                    }

                    var isQualifiedToken = token?.Type == TokenType.Identifier ||
                    token?.Type == TokenType.Number;

                    if (isQualifiedToken &&
                        (token?.Text == ExpressionCode[0].ToString() ||
                        token?.Text == VoiceCode[0].ToString()))
                    {
                        mode = token?.Text == ExpressionCode[0].ToString() ? CastEvaluationMode.Expression : CastEvaluationMode.Voice;

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
                blockStringBuilder.Append("|exp::");
                blockStringBuilder.Append(DoubleQuoteCharacter);
                blockStringBuilder.Append(expression.ToString());
                blockStringBuilder.Append(DoubleQuoteCharacter);
            }

            if (voice != null)
            {
                blockStringBuilder.Append("|vo::");
                blockStringBuilder.Append(DoubleQuoteCharacter);
                blockStringBuilder.Append(voice.ToString());
                blockStringBuilder.Append(DoubleQuoteCharacter);
            }

            blockStringBuilder.Append(CloseCurlyBracketCharacter);

            Content = Content?.ReplaceFirstOccuranceOf(sb.ToString(), blockStringBuilder.ToString());

            SyntaxToken? Peek(int offset, bool includeSpaces = false)
            {
                if (tokens == null) return null;
                var index = _position + offset;

                var filteredTokens = tokens.Skip(index)
                    .Where(token =>
                        (includeSpaces || token?.Type != TokenType.WhiteSpace) &&
                        token?.Type != TokenType.SingleLineComment &&
                        token?.Type != TokenType.MultilineComment);

                return filteredTokens.FirstOrDefault() ?? new SyntaxToken(TokenType.EOF, -1, -1, null, null);
            }


            SyntaxToken? Next()
            {
                _position++;
                return Peek(0, true);
            }
        }

        private void ExtractMacroBlocks()
        {
            if (_tokenCache == null) return;
            if (Content == null) return;
            //TODO: Find Macro Data and extract
            //We're going to replace each block with a control
            //character of /0 to denote a macro is in that position
            var lastTokenPosition = 0;

            for (int i = 0; i < Content.Length; i++)
            {
                var currentCharacter = Content[i];
                if (currentCharacter != OpenCurlyBracketCharacter) continue;
                i = AnalysisMacroStructure(i, lastTokenPosition, out lastTokenPosition);
            }
        }

        private int AnalysisMacroStructure(int position, int tokenPosition, out int lastTokenPosition)
        {
            lastTokenPosition = tokenPosition;

            for (int i = lastTokenPosition; i < _tokenCache?.Length; i++)
            {
                var currentToken = _tokenCache[i];

                if (currentToken.Type == TokenType.OpenCurlyBracket)
                {
                    EvaluateBlock(i, position, out string outputString, out lastTokenPosition);
                    Content = Content?.ReplaceBlockFromPosition(position, outputString);
                    Content = Content?.Remove(position, outputString.Length);
                    position--;
                    return position;
                }
            }
            lastTokenPosition = _tokenCache!.Length - 1;
            return position;
        }

        private void EvaluateBlock(int position, int invocationPosition, out string outputString, out int lastTokenPosition)
        {
            lastTokenPosition = 0;
            outputString = string.Empty;
            if (_tokenCache == null) return;

            Instance = this;

            bool finished = false;
            int length = 0;
            int macroCount = 0;
            int macrosTotal = 0;
            int tokenIndex = 0;

            MacroBlockInfo newBlock = new MacroBlockInfo();
            List<SyntaxToken?> tokenList = new List<SyntaxToken?>();
            StringBuilder sb = new StringBuilder(2048);

            while (!finished)
            {
                SyntaxToken? token = _tokenCache[position + length];

                var tokenType = token.Type;


                if (tokenType == TokenType.WhiteSpace)
                {
                    length++;
                    continue;
                }

                if (tokenType == TokenType.String) sb.Append(DoubleQuoteCharacter);
                sb.Append(token.Text);
                if (tokenType == TokenType.String) sb.Append(DoubleQuoteCharacter);

                tokenList.Add(token);
                macrosTotal += (tokenType == TokenType.Line ? 1 : 0);
                finished = tokenType == TokenType.CloseCurlyBracket;
                length++;
            }

            lastTokenPosition = position + length;

            outputString = sb.ToString();
            macrosTotal += 1;


            finished = false;
            newBlock.Initialize(macrosTotal);
            newBlock.SetPosition(invocationPosition);

            bool multiArgs = false;
            string? macroName = null;

            List<(object value, Type type)> macroArgs = new List<(object, Type)>(2048);
            TokenType? expectingType = TokenType.Identifier | TokenType.DollarSign;

            while (!finished)
            {
                Next();
                SyntaxToken? currentToken = tokenList[tokenIndex];

                if (currentToken?.Type == TokenType.WhiteSpace)
                    continue;

                if (!expectingType.Value.HasFlag(currentToken?.Type))
                {
                    Console.WriteLine("Doesn't Contain Flags...");
                    return;
                }

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

                        if (currentToken.Text?.ToLower() == TrueString ||
                            currentToken.Text?.ToLower() == FalseString)
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
                                        TokenType.DollarSign |
                                        TokenType.OpenParentheses;
                        continue;

                    case TokenType.OpenParentheses:
                        multiArgs = true;
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier |
                                        TokenType.DollarSign;
                        continue;

                    case TokenType.Comma:
                        if (!multiArgs)
                            return;
                        expectingType = TokenType.Number |
                                        TokenType.String |
                                        TokenType.EmptyString |
                                        TokenType.Identifier |
                                        TokenType.DollarSign;
                        continue;

                    case TokenType.CloseParentheses:
                        if (macroName == null)
                            return;

                        expectingType = TokenType.Line | TokenType.CloseCurlyBracket;
                        continue;

                    case TokenType.DollarSign:
                        if (macroName != null)
                        {
                            expectingType = TokenType.Identifier;
                            continue;
                        }
                        SetAsReference(newBlock, macroCount);
                        expectingType = TokenType.Identifier;
                        continue;

                    case TokenType.Line:
                        if (macroName == null)
                            return;

                        expectingType = TokenType.Identifier | TokenType.DollarSign;
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

            void Next() => tokenIndex++;
        }

        private static void PopulateBlock(MacroBlockInfo newBlock, int macroCount, string? macroSymbol, List<(object, Type)> macroArgs)
        {
            if (macroSymbol == null)
            {
                throw new ArgumentNullException(nameof(macroSymbol), "There was no macro symbol present.");
            }

            if (!DefinedMacrosCollection.ValidMacros?.ContainsKey(macroSymbol) == true && newBlock!.macroCalls![macroCount].isReference == false)
            {
                throw new InvalidMacroException($"The macro \"{macroSymbol}\" is undefined.", macroSymbol, Instance!);
            }

            newBlock!.macroCalls![macroCount].macroSymbol = macroSymbol;
            newBlock.macroCalls[macroCount].args = macroArgs.ToArray();
        }

        private void SetAsReference(MacroBlockInfo newBlock, int macroCount)
        {
            newBlock.macroCalls[macroCount].isReference = true;
        }

        private void CleanOutExcessWhiteSpaces()
        {
            if (Content == null) return;
            int i = 0;
            while (i < Content.Length)
            {
                if (Content[i] != NewLineCharacter)
                {
                    i++;
                    continue;
                }

                int peek = 0;
                bool cleared = false;

                while (!cleared)
                {
                    peek++;
                    cleared = !(Content[i + peek] == WhiteSpaceCharacter);
                }
                i++;
                var peekValue = peek - 2;
                if (peekValue < 0)
                {
                    Content = Content.Remove(i, peek - 1);
                    return;
                }
                Content = Content.Remove(i, peekValue);
            }
        }

        private void RemoveReturnCarriages()
        {
            if (Content == null) return;
            Content = Content.Replace(ReturnCharacter.ToString(), string.Empty)
                             .Replace(NewLineCharacter.ToString(), string.Empty)
                             .Replace(TabCharacter.ToString(), string.Empty);
        }

        private void EvaluateNumericType(SyntaxToken? token, out Type? resultType)
        {
            int length = token!.Text!.Length;
            var text = token!.Text;
            var character = text[length - 1];
            if (char.IsLetter(character)) text = text.Remove(length - 1, 1);

            if (char.ToUpper(character) == FloatSuffixCharacter || text.Contains(PeriodCharacter))
            {
                resultType = typeof(float);
                return;
            }

            if (char.ToUpper(character) == DoubleSuffixCharacter || text.Contains(PeriodCharacter))
            {
                resultType = typeof(double);
                return;
            }

            if (char.ToUpper(character) == LongSuffixCharacter)
            {
                resultType = typeof(long);
                return;
            }

            if (char.ToUpper(character) == IntegerSuffixCharacter)
            {
                resultType = typeof(int);
                return;
            }

            character = text[0];

            if (character == DashCharacter)
            {
                resultType = typeof(int);
                return;
            }

            resultType = typeof(uint);
        }

        internal void SetLineTag(string? identifier)
        {
            Name = identifier;
        }

        internal void AppendAtPosition(int cursorIndex, string text)
        {
            _originalContent ??= Content;
            Content = Content?.Insert(cursorIndex, text);
        }

        internal void ShiftMacroCalls(int start, int length)
        {
            if (_modified == true) return;

            _originalMacroList = macroInvocationList;

            for (int i = 0; i < macroInvocationList.Count; i++)
            {
                var macro = macroInvocationList[i];
                if (macro.blockPosition == start) continue;

                macro.blockPosition += length;

                macroInvocationList[i] = macro;
            }
           
            _modified = true;
        }

        internal void Purify()
        {
            if (_modified != true) return;

            Content = _originalContent;
            macroInvocationList = _originalMacroList;
        }
    }
}