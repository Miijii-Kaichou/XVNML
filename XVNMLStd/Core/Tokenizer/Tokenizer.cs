using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XVNML.Core.Enums;

namespace XVNML.Core.Lexer
{
    public static class Tokenizer
    {
        private static readonly int bufferSize = 8192;

        private static int _position = 0;

        private static char _Current
        {
            get
            {
                if (_position < 0 || _position >= SourceText?.Length)
                    return '\0';
                return SourceText![_position];
            }
        }

        public static bool AllowForComplexStructure { get; private set; }
        public static string? SourceText { get; private set; }

        private const int DefaultCapacity = 0xFFFF;

        private static int _Line
        {
            get
            {
                Regex returns = new Regex("\r");
                string substring = SourceText?[.._position]!;
                return substring == string.Empty ? 1 : returns.Matches(substring).Count() + 1;
            }
        }

        private static bool WasThereConflict = false;

        public static List<SyntaxToken?>? Tokenize(string sourceText, TokenizerReadState state, bool complicate = false, int capacity = DefaultCapacity)
        {
            AllowForComplexStructure = complicate;
            SourceText = sourceText;
            switch (state)
            {
                case TokenizerReadState.Local:
                    return TokenizeLocally(capacity);

                case TokenizerReadState.IO:
                    return ReadAndTokenize(capacity);
            }

            return null;
        }

        internal static List<SyntaxToken?> ReadAndTokenize(int capacity)
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

                return TokenizeLocally(capacity);
            }
        }

        internal static List<SyntaxToken?> RemoveRedundantTokens(List<SyntaxToken?> list)
        {
            var definedTokens = list;
            var tokensToRemove = definedTokens
                .Where(t => t.Type == TokenType.EOF ||
                t.Type == TokenType.MultilineComment ||
                t.Type == TokenType.SingleLineComment && t != definedTokens[^1])
                .ToList();
            definedTokens.RemoveAll(tokensToRemove.Contains);
            return definedTokens;
        }

        internal static List<SyntaxToken?> TokenizeLocally(int capacity)
        {
            var definedTokens = new  List<SyntaxToken?>(capacity);

            // Tokenize the entire markup text
            _position = 0;

            SyntaxToken token = new SyntaxToken(default, -1, -1, null, null);
            while (token.Type != TokenType.EOF && !WasThereConflict)
            {
                token = NextToken();
                definedTokens.Add(token);
            }

            return definedTokens;
        }

        public static bool GetConflictState() => WasThereConflict;

        internal static void Next()
        {
            JumpPosition(1);
        }

        internal static SyntaxToken NextToken()
        {
            if (_position >= SourceText?.Length)
            {
                return new SyntaxToken(TokenType.EOF, _Line, _position, null, null);
            }

            if ((_Current == '-' && char.IsDigit(SourceText![_position + 1])) || char.IsDigit(_Current))
            {
                var start = _position;

                while (char.IsDigit(_Current) ||
                      _Current == '.' ||
                      _Current == '-' ||
                      char.ToUpper(_Current) == 'F' ||
                      char.ToUpper(_Current) == 'D' ||
                      char.ToUpper(_Current) == 'L' ||
                      char.ToUpper(_Current) == 'I')
                    Next();


                var text = SourceText?[start.._position];
                var suffix = text![^1];
                var valueString = string.Empty;

                if (char.IsLetter(suffix)) valueString = SourceText?[start..(_position - 1)];

                var finalValueString = valueString != string.Empty ? valueString : text;
                char upperVariant = char.ToUpper(suffix);
                
                if (upperVariant == 'F' || text!.Contains('.'))
                {
                    float.TryParse(finalValueString, out float floatValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, floatValue);
                }

                if (upperVariant == 'D')
                {
                    double.TryParse(finalValueString, out double doubleValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, doubleValue);
                }

                if (upperVariant == 'L')
                {
                    long.TryParse(finalValueString, out long longValue);
                    return new SyntaxToken(TokenType.Number, _Line, start, text, longValue);
                }

                if (text?[0] == '-' || upperVariant == 'I')
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

            #region Unique Tokens
            if (_Current == '<' && AllowForComplexStructure == false)
            {
                var isSelfClosing = false;

                Next();

                var isClosingTag = _Current == '/';

                var start = _position;
                var end = start;
                while (end == 0 || _Current != '>')
                {
                    Next();
                    if (_Current == '/') isSelfClosing = true;
                    end++;
                }

                Next();

                var text = SourceText?[start..end];

                if (isClosingTag)
                    return new SyntaxToken(TokenType.CloseTag, _Line, _position, text, null);

                if (isSelfClosing)
                    return new SyntaxToken(TokenType.SelfTag, _Line, _position, text, null);

                return new SyntaxToken(TokenType.OpenTag, _Line, _position++, text, null);
            }

            if (_Current == '@' && AllowForComplexStructure == false)
            {
                StringBuilder sb = new StringBuilder();
                string endingCharacter = "<";

                sb.Append(_Current);

                Next();

                var start = _position;
                var end = start;
                while (end == 0 || (_Current != '<' && !Peek(_position, "<<")))
                {
                    Next();
                    end++;
                }

                if (Peek(_position, "<<"))
                {
                    endingCharacter = "<<";
                    Next();
                };

                Next();

                var text = SourceText?[start..end];

                sb.Append(text);
                sb.Append(endingCharacter);

                return new SyntaxToken(TokenType.SkriptrDeclarativeLine, _Line, _position++, sb.ToString(),null);
            }

            if (_Current == '?' && AllowForComplexStructure == false)
            {
                StringBuilder sb = new StringBuilder();
                string endingCharacter = ">>";

                sb.Append(_Current);

                Next();

                var start = _position;
                var end = start;
                while (end == 0 || !Peek(_position, ">>"))
                {
                    Next();
                    end++;
                }

                Next();

                var text = SourceText?[start..end];

                sb.Append(text);
                sb.Append(endingCharacter);

                return new SyntaxToken(TokenType.SkriptrInterrogativeLine, _Line, _position++, sb.ToString(), null);
            }
            #endregion

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

        private static bool Peek(int position, string stringSet)
        {
            if (SourceText == null || position < 0 || position + stringSet.Length > SourceText.Length)
            {
                return false;
            }

            var start = position;
            var length = stringSet.Length;
            var end = start + length;

            var proceedingString = SourceText?[start..end];
            return proceedingString!.Equals(stringSet);
        }

        private static void JumpPosition(int distance)
        {
            _position += distance;
        }

        internal static void SetSourceText(string inputString) => SourceText = inputString;
    }
}