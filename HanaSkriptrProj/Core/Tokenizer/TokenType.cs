namespace XVNML.Core.Lexer
{
    public enum TokenType
    {
        //Basic Info
        Invalid,
        OpenBracket,
        CloseBracket,
        Comma,
        Line,
        OpenSquareBracket,
        CloseSquareBracket,
        Colon,
        OpenCurlyBracket,
        CloseCurlyBracket,
        OpenParentheses,
        CloseParentheses,
        String,
        Char,
        Number,
        ForwardSlash,
        BackwardSlash,
        Pound,
        WhiteSpace,
        Identifier,
        EOF,
        SingleLineComment,
        MultilineComment,
        DoubleColon,
        EmptyString,
        Ampersand,
    }
}