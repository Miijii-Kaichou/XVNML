using System.Text.RegularExpressions;

namespace XVNML.Core.Lexer
{
    public class SyntaxToken
    {
        public SyntaxToken(TokenType type, int line, int position, string text, object value)
        {
            Type = type;
            Line = line;
            Position = position;
            Text = text;
            Value = value;
        }

        public TokenType Type { get; }
        public int Line { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
    }

    public class Tokenizer
    {
        private int _position = 0;

        private Action OnNext;

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
                if (_position >= SourceText.Length)
                    return '\0';
                return SourceText[_position];
            }
        }

        public string SourceText { get; private set; }

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
                string substring = SourceText.Substring(0, _position);
                return substring == string.Empty ? 1 : returns.Matches(substring).Count() + 1;
            }
        }

        private bool _isThereConflict = false;

        internal List<SyntaxToken> definedTokens = new List<SyntaxToken>();

        private FileInfo _fileTargetInfo;

        public Tokenizer(string fileTarget, out bool conflictResult)
        {
            _fileTargetInfo = new FileInfo(fileTarget);
            try
            {
                //Read the file only once. This will later be replace by an IO class that will
                //hold the fileTarget's contents prior to Tokenizer and Parser initiation
                using (StreamReader sr = new StreamReader(fileTarget))
                {
                    SourceText = sr.ReadToEnd();
                }

                //Tokenize
                Tokenize(out conflictResult);
            }
            catch (IOException io)
            {
                Console.Write($"Could not launch tokenizer. REASON: {io.Message}");
                conflictResult = true;
                return;
            }
        }

        public void Tokenize(out bool conflictResult)
        {
            Console.WriteLine($"Tokenization of {_fileTargetInfo.Name} starting...\n" +
                $"----------------------------------------------------------------------\n\n");
            while (true)
            {
                var token = NextToken();
                definedTokens.Add(token);
                if (token.Type == TokenType.EOF || _isThereConflict)
                {
                    conflictResult = _isThereConflict;
                    break;
                }

                //Uncomment to debug
                Console.WriteLine($"{token.Type}: '{token.Text}'{(token.Value != null ? $" {token.Value}" : string.Empty)}");
            }
        }

        public void Next()
        {
            JumpPosition(1);
            OnNext?.Invoke();
        }
        public SyntaxToken NextToken()
        {
            if (_position >= SourceText.Length)
            {
                return new SyntaxToken(TokenType.EOF, _Line, _position, "\0", null);
            }

            if (char.IsDigit(_Current))
            {
                var start = _position;

                while (char.IsDigit(_Current))
                    Next();

                var length = _position - start;
                var text = SourceText.Substring(start, length);

                int.TryParse(text, out var value);

                return new SyntaxToken(TokenType.Number, _Line, start, text, value);
            }

            if (char.IsWhiteSpace(_Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(_Current))
                    Next();

                var length = _position - start;
                var text = SourceText.Substring(start, length);

                return new SyntaxToken(TokenType.WhiteSpace, _Line, start, text, null);
            }

            if (char.IsLetter(_Current) || _Current == '_')
            {
                var start = _position;

                while (char.IsLetter(_Current) || char.IsNumber(_Current) || _Current == '_')
                    Next();

                var length = _position - start;
                var text = SourceText.Substring(start, length);

                return new SyntaxToken(TokenType.Identifier, _Line, start, text, text);
            }

            //Comments
            if (Peek(_position, "$>"))
            {
                JumpPosition(2);

                var start = _position;

                while (_Current != '\n')
                    Next();

                var length = _position - start;
                var text = SourceText.Substring(start, length);
                text = text.Trim(' ', '\n', '\r', '\t');

                return new SyntaxToken(TokenType.SingleLineComment, _Line, start, text, text);
            }

            //Multiline Comments
            if (Peek(_position, "$/"))
            {
                JumpPosition(2);

                var start = _position;

                while (Peek(_position, "/$") == false)
                    Next();

                var length = _position - start;
                var text = SourceText.Substring(start, length);
                text = text.Trim(' ', '\n', '\r', '\t');

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

            //Check for DoubleCloseBrackets
            if (Peek(_position, "<<"))
            {
                var start = _position;
                JumpPosition(2);
                return new SyntaxToken(TokenType.DoubleCloseBracket, _Line, start, "<<", null);
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
                var end = 0;
                while (end == 0 || _Current != '\"')
                {
                    Next();
                    end++;
                }

                var text = SourceText.Substring(start, end);

                return new SyntaxToken(TokenType.String, _Line, _position++, text, text);
            }

            // Any Punctuations
            if (_Current == '\'') return new SyntaxToken(TokenType.Apostraphe, _Line, _position++, "'", null);
            if (_Current == '<') return new SyntaxToken(TokenType.OpenBracket, _Line, _position++, "<", null);
            if (_Current == '>') return new SyntaxToken(TokenType.CloseBracket, _Line, _position++, ">", null);
            if (_Current == ',') return new SyntaxToken(TokenType.Comma, _Line, _position++, ",", null);
            if (_Current == '|') return new SyntaxToken(TokenType.Line, _Line, _position++, "|", null);
            if (_Current == '[') return new SyntaxToken(TokenType.OpenSquareBracket, _Line, _position++, "[", null);
            if (_Current == ']') return new SyntaxToken(TokenType.CloseSquareBracket, _Line, _position++, "]", null);
            if (_Current == '{') return new SyntaxToken(TokenType.OpenCurlyBracket, _Line, _position++, "{", null);
            if (_Current == '}') return new SyntaxToken(TokenType.CloseCurlyBracket, _Line, _position++, "}", null);
            if (_Current == ':') return new SyntaxToken(TokenType.Colon, _Line, _position++, ":", null);
            if (_Current == '(') return new SyntaxToken(TokenType.OpenParentheses, _Line, _position++, "(", null);
            if (_Current == ')') return new SyntaxToken(TokenType.CloseParentheses, _Line, _position++, ")", null);
            if (_Current == '/') return new SyntaxToken(TokenType.ForwardSlash, _Line, _position++, "/", null);
            if (_Current == '\\') return new SyntaxToken(TokenType.BackwardSlash, _Line, _position++, "\\", null);
            if (_Current == '#') return new SyntaxToken(TokenType.Pound, _Line, _position++, "#", null);
            if (_Current == '@') return new SyntaxToken(TokenType.At, _Line, _position++, "@", null);
            if (_Current == '?') return new SyntaxToken(TokenType.Prompt, _Line, _position++, "?", null);
            if (_Current == '$') return new SyntaxToken(TokenType.DollarSign, _Line, _position++, "$", null);
            if (_Current == '!') return new SyntaxToken(TokenType.Exclamation, _Line, _position++, "!", null);
            if (_Current == '*') return new SyntaxToken(TokenType.Star, _Line, _position++, "*", null);
            if (_Current == '.') return new SyntaxToken(TokenType.Period, _Line, _position++, ".", null);
            if (_Current == ';') return new SyntaxToken(TokenType.SemiColon, _Line, _position++, ";", null);
            if (_Current == '-') return new SyntaxToken(TokenType.Dash, _Line, _position++, "-", null);
            if (_Current == '_') return new SyntaxToken(TokenType.Equal, _Line, _position++, "_", null);
            if (_Current == '~') return new SyntaxToken(TokenType.Tilda, _Line, _position++, "_", null);
            if (_Current == '`') return new SyntaxToken(TokenType.InverseComma, _Line, _position++, "_", null);
            if (_Current == '%') return new SyntaxToken(TokenType.Percent, _Line, _position++, "_", null);
            if (_Current == '^') return new SyntaxToken(TokenType.Peak, _Line, _position++, "_", null);
            if (_Current == '&') return new SyntaxToken(TokenType.Ampersand, _Line, _position++, "_", null);
            if (_Current == '+') return new SyntaxToken(TokenType.Plus, _Line, _position++, "_", null);
            if (_Current == '=') return new SyntaxToken(TokenType.Equal, _Line, _position++, "_", null);

            _isThereConflict = true;
            return new SyntaxToken(TokenType.Invalid, _Line, _position++, SourceText.Substring(_position - 1, 1), null);
        }

        bool CheckIfEscape(char symbol, out bool result)
        {
            result = symbol == '\\';
            return result;
        }

        bool Peek(int position, string stringSet)
        {
            try
            {
                var start = position;
                var length = stringSet.Length;
                var proceedingString = SourceText.Substring(start, length);
                return proceedingString.Equals(stringSet);
            }
            catch
            {
                return false;
            }
        }

        void JumpPosition(int distance)
        {
            _position += distance;
        }
    }
}