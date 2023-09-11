#pragma warning disable IDE0051 // Remove unused private members

using XVNML.Utilities.Macros;

namespace XVNML.StandardMacroLibrary
{
    [MacroLibrary(typeof(SMLPrint))]
    internal static class SMLPrint
    {

        [Macro("ampersand")]
        [Macro("amper")]
        private static void AmpersandMacro(MacroCallInfo info)
        {
            info.process.Append("&");
        }

        [Macro("asterisk")]
        [Macro("aster")]
        private static void AsterisksMacro(MacroCallInfo info)
        {
            info.process.Append("*");
        }

        [Macro("at")]
        private static void AtMacro(MacroCallInfo info)
        {
            info.process.Append('@');
        }

        [Macro("back_slash")]
        [Macro("blash")]
        private static void BackslashMacro(MacroCallInfo info)
        {
            info.process.Append("\\");
        }

        [Macro("bullet_style_2")]
        [Macro("bul2")]
        private static void Bulle2tMacro(MacroCallInfo info)
        {
            info.process.Append("\u25e6");
        }

        [Macro("bullet_style_3")]
        [Macro("bul3")]
        private static void Bullet3Macro(MacroCallInfo info)
        {
            info.process.Append("\u2023");
        }

        [Macro("bullet_style_1")]
        [Macro("bul1")]
        private static void BulletMacro(MacroCallInfo info)
        {
            info.process.Append("\u2022");
        }

        [Macro("hat")]
        private static void CircumflexMacro(MacroCallInfo info)
        {
            info.process.Append("^");
        }

        [Macro("colon")]
        private static void ColonMacro(MacroCallInfo info)
        {
            info.process.Append(":");
        }

        [Macro("copyright")]
        [Macro("copy")]
        private static void CopyrightMacro(MacroCallInfo info)
        {
            info.process.Append("\u0040");
        }

        [Macro("curly_end")]
        private static void CurlyBracketEndMacro(MacroCallInfo info)
        {
            info.process.Append('}');
        }

        [Macro("curly")]
        private static void CurlyBracketMacro(MacroCallInfo info)
        {
            info.process.Append('{');
        }

        [Macro("degree")]
        [Macro("deg")]
        private static void DegreeMacro(MacroCallInfo info)
        {
            info.process.Append('\u00b0');
        }

        [Macro("plus_minus")]
        [Macro("pm")]
        private static void DegreePlusMinus(MacroCallInfo info)
        {
            info.process.Append('\u00b1');
        }

        [Macro("ellipsis")]
        [Macro("ell")]
        private static void EllipsisMacro(MacroCallInfo info)
        {
            info.process.Append("\u2026");
        }

        [Macro("paren_end")]
        private static void EndParenthesisMacro(MacroCallInfo info)
        {
            info.process.Append(')');
        }

        [Macro("equals")]
        private static void EqualsMacro(MacroCallInfo info)
        {
            info.process.Append('=');
        }

        [Macro("forward_slash")]
        [Macro("slash")]
        private static void ForwardSlashMacro(MacroCallInfo info)
        {
            info.process.Append("/");
        }

        [Macro("hash")]
        private static void HashTagMacro(MacroCallInfo info)
        {
            info.process.Append("#");
        }

        [Macro("speaker")]
        [Macro("sp")]
        private static void InsertSpeakerNameMacro(MacroCallInfo info)
        {
            info.process.Append(info.process.CurrentCastInfo?.name!);
        }
        [Macro("new_line")]
        [Macro("nl")]
        [Macro("n")]
        private static void NewLineMacro(MacroCallInfo info)
        {
            info.process.Append('\n');
        }

        [Macro("paren")]
        private static void ParenthesisMacro(MacroCallInfo info)
        {
            info.process.Append('(');
        }

        [Macro("percent")]
        [Macro("per")]
        private static void PercentMacro(MacroCallInfo info)
        {
            info.process.Append('%');
        }

        [Macro("pipe")]
        private static void PipeMacro(MacroCallInfo info)
        {
            info.process.Append('|');
        }

        [Macro("plus")]
        private static void PlusMacro(MacroCallInfo info)
        {
            info.process.Append('+');
        }

        [Macro("question_mark")]
        [Macro("qm")]
        private static void QuestionMarkMacro(MacroCallInfo info)
        {
            info.process.Append('?');
        }

        [Macro("quot")]
        private static void QuoteMacro(MacroCallInfo info)
        {
            info.process.Append('"');
        }

        [Macro("register_mark")]
        [Macro("reg")]
        private static void RegisteredMacro(MacroCallInfo info)
        {
            info.process.Append("\u00ae");
        }

        [Macro("section")]
        [Macro("sec")]
        private static void SectionMacro(MacroCallInfo info)
        {
            info.process.Append('\u00a7');
        }

        [Macro("semicolon")]
        [Macro("semi")]
        private static void SemicolonMacro(MacroCallInfo info)
        {
            info.process.Append(";");
        }

        [Macro("brack_end")]
        private static void SquareBracketEndMacro(MacroCallInfo info)
        {
            info.process.Append(']');
        }

        [Macro("brack")]
        private static void SquareBracketMacro(MacroCallInfo info)
        {
            info.process.Append('[');
        }

        [Macro("tab")]
        [Macro("tb")]
        [Macro("t")]
        private static void TabMacro(MacroCallInfo info)
        {
            info.process.Append("\t");
        }

        [Macro("tag_end")]
        private static void TagEndMacro(MacroCallInfo info)
        {
            info.process.Append(">");
        }

        [Macro("tag")]
        private static void TagMacro(MacroCallInfo info)
        {
            info.process.Append("<");
        }

        [Macro("tilda")]
        private static void TildaMacro(MacroCallInfo info)
        {
            info.process.Append("~");
        }

        [Macro("trademark")]
        [Macro("trade")]
        private static void TrademarkMacro(MacroCallInfo info)
        {
            info.process.Append("\u2122");
        }

        [Macro("space")]
        [Macro("ws")]
        [Macro("w")]
        private static void WhiteSpaceMacro(MacroCallInfo info)
        {
            info.process.Append(" ");
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members