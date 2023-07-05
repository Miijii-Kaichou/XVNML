using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XVNML.Core.Extensions;

namespace XVNML.Core.Lexer
{
    public enum TokenizerReadState
    {
        Local,
        IO
    }

    public class SyntaxToken
    {
        public SyntaxToken(TokenType type, int line, int position, string? text, object? value)
        {
            Type = type;
            Line = line;
            Position = position;
            Text = text;
            Value = value;
        }

        public TokenType? Type { get; }
        public int? Line { get; }
        public int? Position { get; }
        public string? Text { get; }
        public object? Value { get; }
    }

    public class Tokenizer
    {
        private readonly int bufferSize = 8192;

        private int _position = 0;

        public int Length
        {
            get
            {
                return definedTokens.Count;
            }
        }

        private char _Current
        {
            get
            {
                if (_position >= SourceText?.Length)
                    return '\0';
                return SourceText![_position];
            }
        }

        public string? SourceText { get; private set; }

        public SyntaxToken? this[int index]
        {
            get
            {
                try
                {
                    var token = definedTokens[index];
                    return token;
                }
                catch
                {
                    var eof = definedTokens[Length - 1];
                    return eof;
                }
            }
        }

        private int _Line
        {
            get
            {
                Regex returns = new Regex("\r");
                string substring = SourceText?.Substring(0, _position)!;
                return substring == string.Empty ? 1 : returns.Matches(substring).Count() + 1;
            }
        }

        private bool WasThereConflict = false;

        internal List<SyntaxToken> definedTokens = new List<SyntaxToken>();

        public Tokenizer(string sourceText, TokenizerReadState state)
        {
            SourceText = sourceText;
            switch (state)
            {
                case TokenizerReadState.Local:
                    TokenizeLocally();
                    return;

                case TokenizerReadState.IO:
                    ReadAndTokenize();
                    return;
            }
        }

        private void ReadAndTokenize()
        {
            var sourceText = SourceText;
            SourceText = string.Empty;
            using (StreamReader sr = new StreamReader(sourceText))
            {
                long fileSize = new FileInfo(sourceText).Length;
                StringBuilder sb = new StringBuilder((int)fileSize); // Set expectedTextLength to an appropriate value

                char[] buffer = new char[bufferSize];
                int bytesRead;

                while ((bytesRead = sr.ReadBlock(buffer, 0, bufferSize)) > 0)
                {
                    sb.Append(buffer, 0, bytesRead);
                }
                SourceText = sb.ToString();

                TokenizeLocally();

                // Batch removal of specific tokens
                var tokensToRemove = definedTokens
                    .Where(t => t.Type == TokenType.EOF && t != definedTokens[^1])
                    .ToList();
                definedTokens.RemoveAll(tokensToRemove.Contains);
            }
        }

        internal void TokenizeLocally()
        {
            // Tokenize the entire markup text
            _position = 0;

            SyntaxToken token = new SyntaxToken(default, -1, -1, null, null);
            while (token.Type != TokenType.EOF && !WasThereConflict)
            {
                token = NextToken();
                definedTokens.Add(token);
            }
        }

        public bool GetConflictState() => WasThereConflict;

        public void Next()
        {
            JumpPosition(1);
        }

        public SyntaxToken NextToken()
        {
            if (_position >= SourceText?.Length)
            {
                return new SyntaxToken(TokenType.EOF, _Line, _position, "\0", null);
            }

            if ((_Current == '-' && char.IsDigit(SourceText![_position + 1])) || char.IsDigit(_Current))
            {
                var start = _position;

                bool continueLoop = true;
                while (continueLoop)
                {
                    switch (_Current)
                    {
                        case char c when char.IsDigit(c):
                        case '.':
                        case '-':
                        case 'F':
                        case 'D':
                        case 'L':
                        case 'I':
                            Next();
                            break;

                        default:
                            continueLoop = false;
                            break;
                    }
                }

                var text = SourceText?[start.._position];
                var suffix = text![^1];
                var valueString = string.Empty;

                if (char.IsLetter(suffix)) valueString = SourceText?[start..(_position - 1)];

                var finalValueString = valueString != string.Empty ? valueString : text;

                if (char.ToUpper(suffix) == 'F' || text!.Contains('.'))
                {
                    float.TryParse(finalValueString, out float floatValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, floatValue);
                }

                if (char.ToUpper(suffix) == 'D')
                {
                    double.TryParse(finalValueString, out double doubleValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, doubleValue);
                }

                if (char.ToUpper(suffix) == 'L')
                {
                    long.TryParse(finalValueString, out long longValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, longValue);
                }

                if (text?[0] == '-' || char.ToUpper(suffix) == 'I')
                {
                    int.TryParse(finalValueString, out int intValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, intValue);
                }

                uint.TryParse(finalValueString, out uint uIntValue);
                return new SyntaxToken(TokenType.Number, _Line, start, text, uIntValue);
            }

            if (char.IsWhiteSpace(_Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(_Current))
                    Next();

                var text = SourceText?[start.._position];

                return new SyntaxToken(TokenType.WhiteSpace, _Line, start, text, null);
            }

            if (char.IsLetter(_Current) ||
                _Current == '_')
            {
                var start = _position;
                while (char.IsLetter(_Current) ||
                    _Current == '_' ||
                    char.IsNumber(_Current))
                    Next();

                var text = SourceText?[start.._position];

                return new SyntaxToken(TokenType.Identifier, _Line, start, text, text);
            }

            //Comments
            if (Peek(_position, "$>"))
            {
                JumpPosition(2);

                var start = _position;

                while (_Current != '\n')
                    Next();

                var text = SourceText?[start.._position];
                text = text?.Trim(' ', '\n', '\r', '\t');

                return new SyntaxToken(TokenType.SingleLineComment, _Line, start, text, text);
            }

            //#Region
            if (Peek(_position, "//#"))
            {
                JumpPosition(2);

                var start = _position;

                while (_Current != '\n')
                    Next();

                var text = SourceText?[start.._position];
                text = text?.Trim(' ', '\n', '\r', '\t');

                return new SyntaxToken(TokenType.SingleLineComment, _Line, start, text, text);
            }

            //Multiline Comments
            if (Peek(_position, "$/"))
            {
                JumpPosition(2);

                var start = _position;

                while (Peek(_position, "/$") == false)
                    Next();

                var text = SourceText?[start.._position];
                text = text?.Trim(' ', '\n', '\r', '\t');

                JumpPosition(2);

                return new SyntaxToken(TokenType.MultilineComment, _Line, start, text, text);
            }

            //DoubleColon (Equivalent to = for tags)
            if (Peek(_position, "::"))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleColon, _Line, start, "::", null);
            }

            //Check for DoubleOpenBrackets
            if (Peek(_position, "<<"))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleOpenBracket, _Line, start, "<<", null);
            }

            //Check for DoubleCloseBrackets
            if (Peek(_position, ">>"))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleCloseBracket, _Line, start, ">>", null);
            }

            if (Peek(_position, "???"))
            {
                var start = _position;
                JumpPosition(3);
                return new SyntaxToken(TokenType.AnonymousCastSymbol, _Line, start, "???", null);
            }

            //Find EmptyString
            if (Peek(_position, "\"\""))
            {
                //This is an empty string
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.EmptyString, _Line, start, string.Empty, string.Empty);
            }

            // Find normal string
            if (_Current == '\"')
            {
                Next();

                var start = _position;
                var end = start;
                while (end == 0 || _Current != '\"')
                {
                    Next();
                    end++;
                }

                var text = SourceText?[start..end];

                return new SyntaxToken(TokenType.String, _Line, _position++, text, text);
            }

            // Any Punctuations
            if (_Current == '\'' || _Current == '’') return new SyntaxToken(TokenType.Apostraphe, _Line, _position++, "'", null);
            if (_Current == '<' || _Current == '＜') return new SyntaxToken(TokenType.OpenBracket, _Line, _position++, "<", null);
            if (_Current == '>' || _Current == '＞') return new SyntaxToken(TokenType.CloseBracket, _Line, _position++, ">", null);
            if (_Current == ',' || _Current == '、') return new SyntaxToken(TokenType.Comma, _Line, _position++, ",", null);
            if (_Current == '|' || _Current == '｜') return new SyntaxToken(TokenType.Line, _Line, _position++, "|", null);
            if (_Current == '[' || _Current == '「') return new SyntaxToken(TokenType.OpenSquareBracket, _Line, _position++, "[", null);
            if (_Current == ']' || _Current == '」') return new SyntaxToken(TokenType.CloseSquareBracket, _Line, _position++, "]", null);
            if (_Current == '{' || _Current == '｛') return new SyntaxToken(TokenType.OpenCurlyBracket, _Line, _position++, "{", null);
            if (_Current == '}' || _Current == '｝') return new SyntaxToken(TokenType.CloseCurlyBracket, _Line, _position++, "}", null);
            if (_Current == ':' || _Current == '：') return new SyntaxToken(TokenType.Colon, _Line, _position++, ":", null);
            if (_Current == '(' || _Current == '（') return new SyntaxToken(TokenType.OpenParentheses, _Line, _position++, "(", null);
            if (_Current == ')' || _Current == '）') return new SyntaxToken(TokenType.CloseParentheses, _Line, _position++, ")", null);
            if (_Current == '/' || _Current == '・') return new SyntaxToken(TokenType.ForwardSlash, _Line, _position++, "/", null);
            if (_Current == '\\') return new SyntaxToken(TokenType.BackwardSlash, _Line, _position++, "\\", null);
            if (_Current == '#' || _Current == '＃') return new SyntaxToken(TokenType.Pound, _Line, _position++, "#", null);
            if (_Current == '@' || _Current == '＠') return new SyntaxToken(TokenType.At, _Line, _position++, "@", null);
            if (_Current == '?' || _Current == '？') return new SyntaxToken(TokenType.Prompt, _Line, _position++, "?", null);
            if (_Current == '$' || _Current == '＄' || _Current == '￥') return new SyntaxToken(TokenType.DollarSign, _Line, _position++, "$", null);
            if (_Current == '!' || _Current == '！') return new SyntaxToken(TokenType.Exclamation, _Line, _position++, "!", null);
            if (_Current == '*' || _Current == '＊') return new SyntaxToken(TokenType.Star, _Line, _position++, "*", null);
            if (_Current == '.' || _Current == '。') return new SyntaxToken(TokenType.Period, _Line, _position++, ".", null);
            if (_Current == ';' || _Current == '；') return new SyntaxToken(TokenType.SemiColon, _Line, _position++, ";", null);
            if (_Current == '-' || _Current == 'ー') return new SyntaxToken(TokenType.Dash, _Line, _position++, "-", null);
            if (_Current == '_' || _Current == '＿') return new SyntaxToken(TokenType.Equal, _Line, _position++, "_", null);
            if (_Current == '~' || _Current == '～') return new SyntaxToken(TokenType.Tilda, _Line, _position++, "_", null);
            if (_Current == '`' || _Current == '｀') return new SyntaxToken(TokenType.InverseComma, _Line, _position++, "_", null);
            if (_Current == '%' || _Current == '％') return new SyntaxToken(TokenType.Percent, _Line, _position++, "_", null);
            if (_Current == '^' || _Current == '＾') return new SyntaxToken(TokenType.Peak, _Line, _position++, "_", null);
            if (_Current == '&' || _Current == '＆') return new SyntaxToken(TokenType.Ampersand, _Line, _position++, "_", null);
            if (_Current == '+' || _Current == '＋') return new SyntaxToken(TokenType.Plus, _Line, _position++, "_", null);
            if (_Current == '=' || _Current == '＝') return new SyntaxToken(TokenType.Equal, _Line, _position++, "_", null);

            WasThereConflict = true;
            return new SyntaxToken(TokenType.Invalid, _Line, _position++, SourceText?.Substring(_position - 1, 1), null);
        }

        bool Peek(int position, string stringSet)
        {
            if (SourceText == null || position < 0 || position + stringSet.Length > SourceText.Length)
            {
                return false;
            }

            var start = position;
            var length = stringSet.Length;
            var proceedingString = SourceText?[start..(start + length)];
            return proceedingString!.Equals(stringSet);
        }

        void JumpPosition(int distance)
        {
            _position += distance;
        }

        internal void SetSourceText(string inputString) => SourceText = inputString;
    }
}