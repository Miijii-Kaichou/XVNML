using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XVNML.Core.Enums;
using XVNML.Utilities.Diagnostics;
using static XVNML.CharacterConstants;
using static XVNML.StringConstants;

namespace XVNML.Core.Lexer
{
    public static class Tokenizer
    {
        private static readonly int BufferSize = 8192;

        private static int _position = 0;

        private static char CurrentCharacter
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

        private static int CurrentLine
        {
            get
            {
                Regex returns = new Regex(ReturnCharacter.ToString());
                string? substring = SourceText?[.._position]!;
                return substring == string.Empty ? 1 : returns.Matches(substring).Count() + 1;
            }
        }

        private static bool WasThereConflict = false;

        public static List<SyntaxToken?>? Tokenize(string sourceText, TokenizerReadState state, bool complicate = false, int capacity = DefaultCapacity)
        {
            AllowForComplexStructure = complicate;
            SourceText = sourceText;
            return state switch
            {
                TokenizerReadState.Local => TokenizeLocally(capacity),
                TokenizerReadState.IO => ReadAndTokenize(capacity),
                _ => null,
            };
        }

        internal static List<SyntaxToken?> ReadAndTokenize(int capacity)
        {
            var sourceText = SourceText;
            SourceText = string.Empty;

            using StreamReader sr = new StreamReader(sourceText);

            long fileSize = new FileInfo(sourceText).Length;
            StringBuilder sb = new StringBuilder((int)fileSize);

            char[] buffer = new char[BufferSize];
            int bytesRead;

            while ((bytesRead = sr.ReadBlock(buffer, 0, BufferSize)) > 0)
            {
                sb.Append(buffer, 0, bytesRead);
            }

            SourceText = sb.ToString();

            return TokenizeLocally(capacity);
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
            _position = 0;

            var definedTokens = new List<SyntaxToken?>(capacity);
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
                return new SyntaxToken(TokenType.EOF, CurrentLine, _position, null, null);
            }

            if ((CurrentCharacter == DashCharacter && char.IsDigit(SourceText![_position + 1])) || char.IsDigit(CurrentCharacter))
            {
                var start = _position;

                while (char.IsDigit(CurrentCharacter) ||
                      CurrentCharacter == PeriodCharacter ||
                      CurrentCharacter == DashCharacter ||
                      char.ToUpper(CurrentCharacter) == FloatSuffixCharacter ||
                      char.ToUpper(CurrentCharacter) == DoubleSuffixCharacter ||
                      char.ToUpper(CurrentCharacter) == LongSuffixCharacter ||
                      char.ToUpper(CurrentCharacter) == IntegerSuffixCharacter)
                    Next();


                var text = SourceText?[start.._position];
                var suffix = text![^1];
                var valueString = string.Empty;

                if (char.IsLetter(suffix)) valueString = SourceText?[start..(_position - 1)];

                var finalValueString = valueString != string.Empty ? valueString : text;
                char upperVariant = char.ToUpper(suffix);

                if (upperVariant == FloatSuffixCharacter || text!.Contains(PeriodCharacter))
                {
                    float.TryParse(finalValueString, out float floatValue);
                    return new SyntaxToken(TokenType.Number, CurrentLine, start, text, floatValue);
                }

                if (upperVariant == DoubleSuffixCharacter)
                {
                    double.TryParse(finalValueString, out double doubleValue);
                    return new SyntaxToken(TokenType.Number, CurrentLine, start, text, doubleValue);
                }

                if (upperVariant == LongSuffixCharacter)
                {
                    long.TryParse(finalValueString, out long longValue);
                    return new SyntaxToken(TokenType.Number, CurrentLine, start, text, longValue);
                }

                if (text?[0] == DashCharacter || upperVariant == IntegerSuffixCharacter)
                {
                    int.TryParse(finalValueString, out int intValue);
                    return new SyntaxToken(TokenType.Number, CurrentLine, start, text, intValue);
                }

                uint.TryParse(finalValueString, out uint uIntValue);
                return new SyntaxToken(TokenType.Number, CurrentLine, start, text, uIntValue);
            }

            if (char.IsWhiteSpace(CurrentCharacter))
            {
                var start = _position;

                while (char.IsWhiteSpace(CurrentCharacter))
                    Next();

                string? text = SourceText?[start.._position];

                return new SyntaxToken(TokenType.WhiteSpace, CurrentLine, start, text, null);
            }

            if (char.IsLetter(CurrentCharacter))
            {
                int start = _position;
                while (char.IsLetter(CurrentCharacter) ||
                    CurrentCharacter == UnderscoreCharacter ||
                    char.IsNumber(CurrentCharacter))
                    Next();

                string? text = SourceText?[start.._position];

                return new SyntaxToken(TokenType.Identifier, CurrentLine, start, text, text);
            }

            if (CurrentCharacter == UnderscoreCharacter)
            {
                int start = _position;
                while (char.IsLetter(CurrentCharacter) ||
                    CurrentCharacter == UnderscoreCharacter ||
                    char.IsNumber(CurrentCharacter))
                    Next();

                string? text = SourceText?[start.._position];

                return new SyntaxToken(TokenType.Identifier, CurrentLine, start, text, text);
            }

            //Comments
            if (Peek(_position, SingleLineCommentString))
            {
                JumpPosition(2);

                int start = _position;

                while (CurrentCharacter != NewLineCharacter)
                    Next();

                string? text = SourceText?[start.._position];
                text = text?.Trim(WhiteSpaceCharacter, NewLineCharacter, ReturnCharacter, TabCharacter);

                return new SyntaxToken(TokenType.SingleLineComment, CurrentLine, start, text, text);
            }

            //Reference Identifier
            if (CurrentCharacter == DollarSignCharacter && AllowForComplexStructure == false)
            {
                Next();
                var start = _position;
                while (char.IsLetter(CurrentCharacter) ||
                    CurrentCharacter == UnderscoreCharacter ||
                    char.IsNumber(CurrentCharacter))
                    Next();

                string? text = SourceText?[start.._position];

                return new SyntaxToken(TokenType.ReferenceIdentifier, CurrentLine, start, text, text);
            }

            //#Region
            if (Peek(_position, RegionString))
            {
                JumpPosition(2);

                var start = _position;

                while (CurrentCharacter != NewLineCharacter)
                    Next();

                var text = SourceText?[start.._position];
                text = text?.Trim(WhiteSpaceCharacter, NewLineCharacter, ReturnCharacter, TabCharacter);

                return new SyntaxToken(TokenType.SingleLineComment, CurrentLine, start, text, text);
            }

            //Multiline Comments
            if (Peek(_position, MultiLineOpenCommentString))
            {
                JumpPosition(2);

                var start = _position;

                while (Peek(_position, MultiLineCloseCommentString) == false)
                    Next();

                var text = SourceText?[start.._position];
                text = text?.Trim(WhiteSpaceCharacter, NewLineCharacter, ReturnCharacter, TabCharacter);

                JumpPosition(2);

                return new SyntaxToken(TokenType.MultilineComment, CurrentLine, start, text, text);
            }

            //DoubleColon (Equivalent to = for tags)
            if (Peek(_position, DoubleColonString))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleColon, CurrentLine, start, DoubleColonString, null);
            }

            //Check for DoubleOpenBrackets
            if (Peek(_position, DoubleLessThanString))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleOpenBracket, CurrentLine, start, DoubleLessThanString, null);
            }

            //Check for DoubleCloseBrackets
            if (Peek(_position, DoubleGreaterThanString))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleCloseBracket, CurrentLine, start, DoubleGreaterThanString, null);
            }

            if (Peek(_position, AnonymousString))
            {
                var start = _position;
                JumpPosition(3);
                return new SyntaxToken(TokenType.AnonymousCastSymbol, CurrentLine, start, AnonymousString, null);
            }

            //Find EmptyString
            if (Peek(_position, EmptyString))
            {
                //This is an empty string
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.EmptyString, CurrentLine, start, string.Empty, string.Empty);
            }

            // Find normal string
            // TODO: Handle the worse case scenario when it never finds the ending quotation mark.
            if (CurrentCharacter == DoubleQuoteCharacter)
            {
                Next();

                var start = _position;
                var end = start;

                while (end == 0 || CurrentCharacter != DoubleQuoteCharacter)
                {
                    Next();
                    end++;
                }


                var text = SourceText?[start..end];

                return new SyntaxToken(TokenType.String, CurrentLine, _position++, text, text);
            }

            #region Unique Tokens
            if (CurrentCharacter == LessThanCharacter && AllowForComplexStructure == false)
            {
                var isSelfClosing = false;

                Next();

                var isClosingTag = CurrentCharacter == ForwardSlashCharacter;

                var start = _position;
                var end = start;
                while (end == 0 || CurrentCharacter != GreaterThanCharacter)
                {
                    Next();
                    if (CurrentCharacter == ForwardSlashCharacter) isSelfClosing = true;
                    end++;
                }

                Next();

                var text = SourceText?[start..end];

                if (isClosingTag)
                    return new SyntaxToken(TokenType.CloseTag, CurrentLine, _position, text, null);

                if (isSelfClosing)
                    return new SyntaxToken(TokenType.SelfTag, CurrentLine, _position, text, null);

                return new SyntaxToken(TokenType.OpenTag, CurrentLine, _position++, text, null);
            }

            if (CurrentCharacter == AtCharacter && AllowForComplexStructure == false)
            {
                StringBuilder sb = new StringBuilder();
                string endingCharacter = LessThanCharacter.ToString();

                sb.Append(CurrentCharacter);

                Next();

                var start = _position;
                var end = start;
                while (end == 0 || (CurrentCharacter != LessThanCharacter && !Peek(_position, DoubleLessThanString)))
                {
                    Next();
                    end++;
                }

                if (Peek(_position, DoubleLessThanString))
                {
                    endingCharacter = DoubleLessThanString;
                    Next();
                };

                Next();

                var text = SourceText?[start..end];

                sb.Append(text);
                sb.Append(endingCharacter);

                return new SyntaxToken(TokenType.SkriptrDeclarativeLine, CurrentLine, _position++, sb.ToString(), null);
            }

            if (CurrentCharacter == QuestionMarkCharacter && AllowForComplexStructure == false)
            {
                StringBuilder sb = new StringBuilder();
                string endingCharacter = DoubleGreaterThanString;

                sb.Append(CurrentCharacter);

                Next();

                var start = _position;
                var end = start;
                while (end == 0 || !Peek(_position, endingCharacter))
                {
                    Next();
                    end++;
                }

                Next();

                var text = SourceText?[start..end];

                sb.Append(text);
                sb.Append(endingCharacter);

                return new SyntaxToken(TokenType.SkriptrInterrogativeLine, CurrentLine, _position++, sb.ToString(), null);
            }
            #endregion

            // Any Punctuations
            if (CurrentCharacter == '\'' || CurrentCharacter == '’')
                return new SyntaxToken(TokenType.Apostraphe, CurrentLine, _position++, "'", null);
            if (CurrentCharacter == '<' || CurrentCharacter == '＜')
                return new SyntaxToken(TokenType.OpenBracket, CurrentLine, _position++, "<", null);
            if (CurrentCharacter == '>' || CurrentCharacter == '＞')
                return new SyntaxToken(TokenType.CloseBracket, CurrentLine, _position++, ">", null);
            if (CurrentCharacter == ',' || CurrentCharacter == '、')
                return new SyntaxToken(TokenType.Comma, CurrentLine, _position++, ",", null);
            if (CurrentCharacter == '|' || CurrentCharacter == '｜')
                return new SyntaxToken(TokenType.Line, CurrentLine, _position++, "|", null);
            if (CurrentCharacter == '[' || CurrentCharacter == '「')
                return new SyntaxToken(TokenType.OpenSquareBracket, CurrentLine, _position++, "[", null);
            if (CurrentCharacter == ']' || CurrentCharacter == '」')
                return new SyntaxToken(TokenType.CloseSquareBracket, CurrentLine, _position++, "]", null);
            if (CurrentCharacter == '{' || CurrentCharacter == '｛')
                return new SyntaxToken(TokenType.OpenCurlyBracket, CurrentLine, _position++, "{", null);
            if (CurrentCharacter == '}' || CurrentCharacter == '｝')
                return new SyntaxToken(TokenType.CloseCurlyBracket, CurrentLine, _position++, "}", null);
            if (CurrentCharacter == ':' || CurrentCharacter == '：')
                return new SyntaxToken(TokenType.Colon, CurrentLine, _position++, ":", null);
            if (CurrentCharacter == '(' || CurrentCharacter == '（')
                return new SyntaxToken(TokenType.OpenParentheses, CurrentLine, _position++, "(", null);
            if (CurrentCharacter == ')' || CurrentCharacter == '）')
                return new SyntaxToken(TokenType.CloseParentheses, CurrentLine, _position++, ")", null);
            if (CurrentCharacter == '/' || CurrentCharacter == '・')
                return new SyntaxToken(TokenType.ForwardSlash, CurrentLine, _position++, "/", null);
            if (CurrentCharacter == '\\')
                return new SyntaxToken(TokenType.BackwardSlash, CurrentLine, _position++, "\\", null);
            if (CurrentCharacter == '#' || CurrentCharacter == '＃')
                return new SyntaxToken(TokenType.Pound, CurrentLine, _position++, "#", null);
            if (CurrentCharacter == '@' || CurrentCharacter == '＠')
                return new SyntaxToken(TokenType.At, CurrentLine, _position++, "@", null);
            if (CurrentCharacter == '?' || CurrentCharacter == '？')
                return new SyntaxToken(TokenType.Prompt, CurrentLine, _position++, "?", null);
            if (CurrentCharacter == '$' || CurrentCharacter == '＄' || CurrentCharacter == '￥')
                return new SyntaxToken(TokenType.DollarSign, CurrentLine, _position++, "$", null);
            if (CurrentCharacter == '!' || CurrentCharacter == '！')
                return new SyntaxToken(TokenType.Exclamation, CurrentLine, _position++, "!", null);
            if (CurrentCharacter == '*' || CurrentCharacter == '＊')
                return new SyntaxToken(TokenType.Star, CurrentLine, _position++, "*", null);
            if (CurrentCharacter == '.' || CurrentCharacter == '。')
                return new SyntaxToken(TokenType.Period, CurrentLine, _position++, ".", null);
            if (CurrentCharacter == ';' || CurrentCharacter == '；')
                return new SyntaxToken(TokenType.SemiColon, CurrentLine, _position++, ";", null);
            if (CurrentCharacter == '-' || CurrentCharacter == 'ー')
                return new SyntaxToken(TokenType.Dash, CurrentLine, _position++, "-", null);
            if (CurrentCharacter == UnderscoreCharacter || CurrentCharacter == '＿')
                return new SyntaxToken(TokenType.Underscore, CurrentLine, _position++, "_", null);
            if (CurrentCharacter == '~' || CurrentCharacter == '～')
                return new SyntaxToken(TokenType.Tilda, CurrentLine, _position++, "~", null);
            if (CurrentCharacter == '`' || CurrentCharacter == '｀')
                return new SyntaxToken(TokenType.InverseComma, CurrentLine, _position++, "`", null);
            if (CurrentCharacter == '%' || CurrentCharacter == '％')
                return new SyntaxToken(TokenType.Percent, CurrentLine, _position++, "%", null);
            if (CurrentCharacter == '^' || CurrentCharacter == '＾')
                return new SyntaxToken(TokenType.Peak, CurrentLine, _position++, "^", null);
            if (CurrentCharacter == '&' || CurrentCharacter == '＆')
                return new SyntaxToken(TokenType.Ampersand, CurrentLine, _position++, "&", null);
            if (CurrentCharacter == '+' || CurrentCharacter == '＋')
                return new SyntaxToken(TokenType.Plus, CurrentLine, _position++, "+", null);
            if (CurrentCharacter == '=' || CurrentCharacter == '＝')
                return new SyntaxToken(TokenType.Equal, CurrentLine, _position++, "=", null);

            WasThereConflict = true;
            return new SyntaxToken(TokenType.Invalid, CurrentLine, _position++, SourceText?.Substring(_position - 1, 1), null);
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