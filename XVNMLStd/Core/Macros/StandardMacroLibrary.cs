using System;
using System.Linq;
using System.Text;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Macros;

[MacroLibrary(typeof(StandardMacroLibrary))]
internal sealed class StandardMacroLibrary
{
    #region Control Macros
    [Macro("del")]
    [Macro("delay")]
    internal static void DelayMacro(MacroCallInfo info, uint milliseconds)
    {
        // Delay macro logic here.
        info.process.Wait(milliseconds);
    }

    [Macro("ins")]
    [Macro("insert")]
    internal static void InsertMacro(MacroCallInfo info, string text)
    {
        byte[] textBytes = Encoding.Unicode.GetBytes(text);
        var finalText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, textBytes));

        // Insert macro logic here.
        info.process.Append(finalText.ToString());
    }

    [Macro("sts")]
    [Macro("set_text_speed")]
    internal static void SetTextSpeed(MacroCallInfo info, uint level)
    {
        // Speed macro logic here.
        info.process.SetProcessRate(level == 0 ? level : 1000 / level);
    }

    [Macro("clr")]
    [Macro("clear")]
    internal static void ClearText(MacroCallInfo info)
    {
        info.process.Clear();
    }

    [Macro("pause")]
    internal static void PauseMacro(MacroCallInfo info)
    {
        info.process.Pause();
    }

    [Macro("pass")]
    internal static void PassMacro(MacroCallInfo info)
    {
        info.process.AllowPass();
    }

    [Macro("op")]
    internal static void OperationsVariableMacro(MacroCallInfo info, string expression)
    {
        
    }

    [Macro("jmpt")]
    [Macro("jump_to")]
    internal static void JumpToMacro(MacroCallInfo info, uint index)
    {
        info.process.JumpTo((int)index);
    }

    [Macro("jmpt")]
    [Macro("jump_to")]
    internal static void JumpToMacro(MacroCallInfo info, string tagName)
    {
        if (info.process.lineProcesses.Where(sl => sl.Name == tagName.ToString()).Any() == false) return;

        info.process.JumpTo(tagName.ToString());
    }

    [Macro("ldt")]
    [Macro("lead_to")]
    internal static void LeadToLineMacro(MacroCallInfo info, int value)
    {
        info.process.LeadTo(value);
    }

    [Macro("end")]
    internal static void EndDialogueMacro(MacroCallInfo info)
    {
        info.process.lineIndex = info.process.lineProcesses.Count;
    }

    #endregion
    
    #region Debug Macros
    [Macro("pid")]
    internal static void GetProcessIDMacroShortHand(MacroCallInfo info, bool print)
    {
        GetProcessIDMacro(info, print);
    }

    [Macro("process_id")]
    internal static void GetProcessIDMacro(MacroCallInfo info, bool print)
    {
        if (!print) return;
        info.process.Append(info.process.ID.ToString());
    }

    [Macro("curdex")]
    internal static void CursorIndexMacro(MacroCallInfo info, bool print)
    {
        var cursorIndex = info.process.cursorIndex;
        XVNMLLogger.Log(cursorIndex.ToString(), info);
        if (!print) return;
        info.process.Append(cursorIndex.ToString());
    }

    [Macro("lindex")]
    internal static void GetLineIndexMacro(MacroCallInfo info)
    {
        var print = false;
        GetLineIndexMacro(info, print);
    }

    [Macro("lindex")]
    internal static void GetLineIndexMacro(MacroCallInfo info, bool print)
    {
        var lineIndex = info.process.lineIndex;
        XVNMLLogger.Log(lineIndex.ToString(), info);
        if (!print) return;
        info.process.Append(lineIndex.ToString());
    }

    #endregion

    #region Character Insert (Print) Macros
    [Macro("new_line")]
    [Macro("nl")]
    [Macro("n")]
    internal static void NewLineMacro(MacroCallInfo info)
    {
        info.process.Append('\n');
    }

    [Macro("tab")]
    [Macro("tb")]
    [Macro("t")]
    internal static void TabMacro(MacroCallInfo info)
    {
        info.process.Append("\t");
    }

    [Macro("space")]
    [Macro("ws")]
    [Macro("w")]
    internal static void WhiteSpaceMacro(MacroCallInfo info)
    {
        info.process.Append(" ");
    }

    [Macro("hash")]
    internal static void HashTagMacro(MacroCallInfo info)
    {
        info.process.Append("#");
    }

    [Macro("p")]
    [Macro("paren")]
    internal static void ParenthesisMacro(MacroCallInfo info)
    {
        info.process.Append('(');
    }

    [Macro("/p")]
    [Macro("paren_end")]
    internal static void EndParenthesisMacro(MacroCallInfo info)
    {
        info.process.Append(')');
    }

    [Macro("quot")]
    internal static void QuoteMacro(MacroCallInfo info)
    {
        info.process.Append('"');
    }

    [Macro("curly")]
    internal static void CurlyBracketMacro(MacroCallInfo info)
    {
        info.process.Append('{');
    }

    [Macro("/curly")]
    [Macro("curly_end")]
    internal static void CurlyBracketEndMacro(MacroCallInfo info)
    {
        info.process.Append('}');
    }

    [Macro("brack")]
    internal static void SquareBracketMacro(MacroCallInfo info)
    {
        info.process.Append('[');
    }

    [Macro("/brack")]
    [Macro("brack_end")]
    internal static void SquareBracketEndMacro(MacroCallInfo info)
    {
        info.process.Append(']');
    }

    [Macro("pipe")]
    internal static void PipeMacro(MacroCallInfo info)
    {
        info.process.Append('|');
    }

    [Macro("asterisk")]
    [Macro("aster")]
    internal static void AsterisksMacro(MacroCallInfo info)
    {
        info.process.Append("*");
    }

    [Macro("ampersand")]
    [Macro("amper")]
    internal static void AmpersandMacro(MacroCallInfo info)
    {
        info.process.Append("&");
    }

    [Macro("hat")]
    internal static void CircumflexMacro(MacroCallInfo info)
    {
        info.process.Append("^");
    }

    [Macro("tilda")]
    internal static void TildaMacro(MacroCallInfo info)
    {
        info.process.Append("~");
    }

    [Macro("forward_slash")]
    [Macro("slash")]
    internal static void ForwardSlashMacro(MacroCallInfo info)
    {
        info.process.Append("/");
    }

    [Macro("back_slash")]
    [Macro("blash")]
    internal static void BackslashMacro(MacroCallInfo info)
    {
        info.process.Append("\\");
    }

    [Macro("semicolon")]
    [Macro("semi")]
    internal static void SemicolonMacro(MacroCallInfo info)
    {
        info.process.Append(";");
    }

    [Macro("colon")]
    internal static void ColonMacro(MacroCallInfo info)
    {
        info.process.Append(":");
    }

    [Macro("tag")]
    internal static void TagMacro(MacroCallInfo info)
    {
        info.process.Append("<");
    }

    [Macro("/tag")]
    [Macro("tag_end")]
    internal static void TagEndMacro(MacroCallInfo info)
    {
        info.process.Append(">");
    }

    [Macro("percent")]
    [Macro("per")]
    internal static void PercentMacro(MacroCallInfo info)
    {
        info.process.Append('%');
    }

    [Macro("plus")]
    internal static void PlusMacro(MacroCallInfo info)
    {
        info.process.Append('+');
    }

    [Macro("equals")]
    internal static void EqualsMacro(MacroCallInfo info)
    {
        info.process.Append('=');
    }

    [Macro("at")]
    internal static void AtMacro(MacroCallInfo info)
    {
        info.process.Append('@');
    }

    [Macro("question_mark")]
    [Macro("qm")]
    internal static void QuestionMarkMacro(MacroCallInfo info)
    {
        info.process.Append('?');
    }

    [Macro("trademark")]
    [Macro("trade")]
    internal static void TrademarkMacro(MacroCallInfo info)
    {
        info.process.Append("\u2122");
    }

    [Macro("copyright")]
    [Macro("copy")]
    internal static void CopyrightMacro(MacroCallInfo info)
    {
        info.process.Append("\u0040");
    }

    [Macro("register_mark")]
    [Macro("reg")]
    internal static void RegisteredMacro(MacroCallInfo info)
    {
        info.process.Append("\u00ae");
    }

    [Macro("bullet_style_1")]
    [Macro("bul1")]
    internal static void BulletMacro(MacroCallInfo info)
    {
        info.process.Append("\u2022");
    }

    [Macro("bullet_style_2")]
    [Macro("bul2")]
    internal static void Bulle2tMacro(MacroCallInfo info)
    {
        info.process.Append("\u25e6");
    }

    [Macro("bullet_style_3")]
    [Macro("bul3")]
    internal static void Bullet3Macro(MacroCallInfo info)
    {
        info.process.Append("\u2023");
    }

    [Macro("ellipsis")]
    [Macro("ell")]
    internal static void EllipsisMacro(MacroCallInfo info)
    {
        info.process.Append("\u2026");
    }

    [Macro("section")]
    [Macro("sec")]
    internal static void SectionMacro(MacroCallInfo info)
    {
        info.process.Append('\u00a7');
    }

    [Macro("degree")]
    [Macro("deg")]
    internal static void DegreeMacro(MacroCallInfo info)
    {
        info.process.Append('\u00b0');
    }

    [Macro("plus_minus")]
    [Macro("pm")]
    internal static void DegreePlusMinus(MacroCallInfo info)
    {
        info.process.Append('\u00b1');
    }

    [Macro("speaker")]
    [Macro("sp")]
    internal static void InsertSpeakerNameMacro(MacroCallInfo info)
    {
        info.process.Append(info.process.CurrentCastInfo?.name!);
    }

    #endregion

    #region Cast Macros
    [Macro("expression")]
    [Macro("portrait")]
    [Macro("exp")]
    [Macro("port")]
    internal static void SetCastExpressionMacro(MacroCallInfo info, string value)
    {
        info.process.ChangeCastExpression(info, value);
    }

    [Macro("expression")]
    [Macro("portrait")]
    [Macro("exp")]
    [Macro("port")]
    internal static void SetCastExpressionMacro(MacroCallInfo info, int value)
    {
        SetCastExpressionMacro(info, value.ToString());
    }

    [Macro("voice")]
    [Macro("vo")]
    internal static void SetCastVoice(MacroCallInfo info, string value)
    {
        info.process.ChangeCastVoice(info, value);
    }

    [Macro("voice")]
    [Macro("vo")]
    internal static void SetCastVoice(MacroCallInfo info, int value)
    {
        SetCastVoice(info, value.ToString());
    }
    #endregion

    #region Variable Control Macros
    [Macro("declare")]
    internal static void InitializeVariableMacro(MacroCallInfo info, string identifier, object initialValue)
    {
        Console.WriteLine($"Variable {identifier} initiated with {initialValue}");
    }

    [Macro("set")]
    internal static void SetVariableMacro(MacroCallInfo info, string identifier, object newValue)
    {

    }

    [Macro("get")]
    internal static void GetVariableMacro(MacroCallInfo info, string identifier)
    {

    }
    #endregion
}