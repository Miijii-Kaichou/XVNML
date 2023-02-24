// See https://aka.ms/new-console-template for more information


using XVNML.Core.Dialogue;
using XVNML.Utility.Macros;
using XVNML.Utility.Dialogue;
using System.Threading;
using System.Text;

[MacroLibrary(typeof(StandardMacroLibrary))]
internal static class StandardMacroLibrary
{
    #region Control Macros
    [Macro("delay")]
    internal static void DelayMacro(MacroCallInfo info, uint milliseconds)
    {
        // Delay macro logic here.
        info.source.Wait(milliseconds);
    }

    [Macro("insert")]
    internal static void InsertMacro(MacroCallInfo info, string text)
    {
        byte[] textBytes = Encoding.Unicode.GetBytes(text); 
        var finalText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, textBytes));
        
        // Insert macro logic here.
        info.source.Append(finalText.ToString());
    }

    [Macro("set_text_speed")]
    internal static void SetTextSpeed(MacroCallInfo info, uint level)
    {
        // Speed macro logic here.
        info.source.SetProcessRate(level == 0 ? level : 1000 / level);
    }

    [Macro("clear")]
    internal static void ClearText(MacroCallInfo info)
    {
        info.source.Clear();
    }

    [Macro("pause")]
    internal static void PauseMacro(MacroCallInfo info)
    {
        info.source.Pause();
    }
    #endregion

    #region Character Insert Macros
    [Macro("new_line")]
    internal static void NewLineMacro(MacroCallInfo info)
    {
        info.source.Append('\n');
    }

    [Macro("paren")]
    internal static void ParenthesisMacro(MacroCallInfo info)
    {
        info.source.Append('(');
    }

    [Macro("paren_end")]
    internal static void EndParenthesisMacro(MacroCallInfo info)
    {
        info.source.Append(')');
    }

    [Macro("curly")]
    internal static void CurlyBracketMacro(MacroCallInfo info)
    {
        info.source.Append('{');
    }

    [Macro("curly_end")]
    internal static void CurlyBracketEndMacro(MacroCallInfo info)
    {
        info.source.Append('}');
    }

    [Macro("pipe")]
    internal static void PipeMacro(MacroCallInfo info)
    {
        info.source.Append('|');
    }

    [Macro("aster")]
    internal static void AsterisksMacro(MacroCallInfo info)
    {
        info.source.Append("*");
    }

    [Macro("amper")]
    internal static void AmpersandMacro(MacroCallInfo info)
    {
        info.source.Append("&");
    }

    [Macro("hat")]
    internal static void CircumflexMacro(MacroCallInfo info)
    {
        info.source.Append("^");
    }

    [Macro("tilda")]
    internal static void TildaMacro(MacroCallInfo info)
    {
        info.source.Append("~");
    }

    [Macro("slash")]
    internal static void ForwardSlashMacro(MacroCallInfo info)
    {
        info.source.Append("/");
    }

    [Macro("blash")]
    internal static void BackslashMacro(MacroCallInfo info)
    {
        info.source.Append("\\");
    }

    [Macro("semi")]
    internal static void SemicolonMacro(MacroCallInfo info)
    {
        info.source.Append(";");
    }

    [Macro("tag")]
    internal static void TagMacro(MacroCallInfo info)
    {
        info.source.Append("<");
    }

    [Macro("tag_end")]
    internal static void TagEndMacro(MacroCallInfo info)
    {
        info.source.Append(">");
    }

    [Macro("per")]
    internal static void PercentMacro(MacroCallInfo info)
    {
        info.source.Append('%');
    }

    [Macro("trade")]
    internal static void TrademarkMacro(MacroCallInfo info)
    {
        info.source.Append("\u2122");
    }

    [Macro("copy")]
    internal static void CopyrightMacro(MacroCallInfo info)
    {
        info.source.Append("\u0040");
    }

    [Macro("reg")]
    internal static void RegisteredMacro(MacroCallInfo info)
    {
        info.source.Append("\u00ae");
    }

    [Macro("bul1")]
    internal static void BulletMacro(MacroCallInfo info)
    {
        info.source.Append("\u2022");
    }

    [Macro("bul2")]
    internal static void Bulle2tMacro(MacroCallInfo info)
    {
        info.source.Append("\u25e6");
    }

    [Macro("bul3")]
    internal static void Bullet3Macro(MacroCallInfo info)
    {
        info.source.Append("\u2023");
    }

    [Macro("ell")]
    internal static void EllipsisMacro(MacroCallInfo info)
    {
        info.source.Append("\u2026");
    }

    [Macro("sec")]
    internal static void SectionMacro(MacroCallInfo info)
    {
        info.source.Append('\u00a7');
    }

    [Macro("deg")]
    internal static void DegreeMacro(MacroCallInfo info)
    {
        info.source.Append('\u00b0');
    }

    [Macro("pm")]
    internal static void DegreePlusMinus(MacroCallInfo info)
    {
        info.source.Append('\u00b1');
    }
    #endregion
}