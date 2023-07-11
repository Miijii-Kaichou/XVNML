using XVNML.Core.Enums;

namespace XVNML.Core.Lexer
{
    public sealed class SyntaxToken
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
}