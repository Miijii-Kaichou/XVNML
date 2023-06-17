﻿using System.Text;
using System.Linq;
using XVNML.Utility.Macros;
using XVNML.Core.Dialogue.Structs;
using System;

[MacroLibrary(typeof(StandardMacroLibrary))]
internal static class StandardMacroLibrary
{
    #region Control Macros
    [Macro("del")]
    internal static void DelayMacroShortHand(MacroCallInfo info, uint milliseconds)
    {
        DelayMacro(info, milliseconds);
    }
    [Macro("delay")]
    internal static void DelayMacro(MacroCallInfo info, uint milliseconds)
    {
        // Delay macro logic here.
        info.process.Wait(milliseconds);
    }

    [Macro("ins")]
    internal static void InsertMacroShortHand(MacroCallInfo info, string text)
    {
        InsertMacro(info, text);
    }

    [Macro("insert")]
    internal static void InsertMacro(MacroCallInfo info, string text)
    {
        byte[] textBytes = Encoding.Unicode.GetBytes(text);
        var finalText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, textBytes));

        // Insert macro logic here.
        info.process.Append(finalText.ToString());
    }

    [Macro("sts")]
    internal static void SetTextSpeedShortHand(MacroCallInfo info, uint level)
    {
        // Speed macro logic here.
        SetTextSpeed(info, level);
    }

    [Macro("set_text_speed")]
    internal static void SetTextSpeed(MacroCallInfo info, uint level)
    {
        // Speed macro logic here.
        info.process.SetProcessRate(level == 0 ? level : 1000 / level);
    }

    [Macro("clr")]
    internal static void ClearTextShortHand(MacroCallInfo info)
    {
        ClearText(info);
    }
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

    [Macro("jtbi")]
    internal static void JumpToLineMacroShortHand(MacroCallInfo info, uint index)
    {
        JumpToLineMacro(info, index);
    }

    [Macro("jump_to_by_index")]
    internal static void JumpToLineMacro(MacroCallInfo info, uint index)
    {
        info.process.JumpTo((int)index);
        return;
    }

    [Macro("jtbt")]
    internal static void JumpToLineMacroShortHand(MacroCallInfo info, string tagName)
    {
        JumpToLineMacro(info, tagName);
    }

    [Macro("jump_to_by_tag")]
    internal static void JumpToLineMacro(MacroCallInfo info, string tagName)
    {
        if (info.process.lineProcesses.Where(sl => sl.TaggedAs == tagName.ToString()).Any() == false) return;

        info.process.JumpTo(tagName.ToString());
    }

    [Macro("lead_to")]
    internal static void LeadToLineMacro(MacroCallInfo info, int value)
    {
        info.process.LeadTo(value);
    }

    [Macro("end")]
    internal static void EndDialogueMacro(MacroCallInfo info)
    {
        info.process.lineProcessIndex = info.process.lineProcesses.Count;
    }

    #endregion

    #region Character Insert Macros
    [Macro("n")]
    internal static void NewLineMacroShortHand1(MacroCallInfo info)
    {
        NewLineMacro(info);
    }
    [Macro("nl")]
    internal static void NewLineMacroShortHand2(MacroCallInfo info)
    {
        NewLineMacro(info);
    }
    [Macro("new_line")]
    internal static void NewLineMacro(MacroCallInfo info)
    {
        info.process.Append('\n');
    }

    [Macro("t")]
    internal static void TabMacroShortHand1(MacroCallInfo info)
    {
        TabMacro(info);
    }
    [Macro("tb")]
    internal static void TabMacroShortHand2(MacroCallInfo info)
    {
        TabMacro(info);
    }
    [Macro("tab")]
    internal static void TabMacro(MacroCallInfo info)
    {
        info.process.Append("\t");
    }

    [Macro("w")]
    internal static void WhiteSpaceMacroShortHand1(MacroCallInfo info)
    {
        WhiteSpaceMacro(info);
    }
    [Macro("ws")]
    internal static void WhiteSpaceMacroShortHand2(MacroCallInfo info)
    {
        WhiteSpaceMacro(info);
    }
    [Macro("space")]
    internal static void WhiteSpaceMacro(MacroCallInfo info)
    {
        info.process.Append(" ");
    }

    [Macro("paren")]
    internal static void ParenthesisMacro(MacroCallInfo info)
    {
        info.process.Append('(');
    }

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

    [Macro("aster")]
    internal static void AsterisksMacro(MacroCallInfo info)
    {
        info.process.Append("*");
    }

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

    [Macro("slash")]
    internal static void ForwardSlashMacro(MacroCallInfo info)
    {
        info.process.Append("/");
    }

    [Macro("blash")]
    internal static void BackslashMacro(MacroCallInfo info)
    {
        info.process.Append("\\");
    }

    [Macro("semi")]
    internal static void SemicolonMacro(MacroCallInfo info)
    {
        info.process.Append(";");
    }

    [Macro("tag")]
    internal static void TagMacro(MacroCallInfo info)
    {
        info.process.Append("<");
    }

    [Macro("tag_end")]
    internal static void TagEndMacro(MacroCallInfo info)
    {
        info.process.Append(">");
    }

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

    [Macro("trade")]
    internal static void TrademarkMacro(MacroCallInfo info)
    {
        info.process.Append("\u2122");
    }

    [Macro("copy")]
    internal static void CopyrightMacro(MacroCallInfo info)
    {
        info.process.Append("\u0040");
    }

    [Macro("reg")]
    internal static void RegisteredMacro(MacroCallInfo info)
    {
        info.process.Append("\u00ae");
    }

    [Macro("bul1")]
    internal static void BulletMacro(MacroCallInfo info)
    {
        info.process.Append("\u2022");
    }

    [Macro("bul2")]
    internal static void Bulle2tMacro(MacroCallInfo info)
    {
        info.process.Append("\u25e6");
    }

    [Macro("bul3")]
    internal static void Bullet3Macro(MacroCallInfo info)
    {
        info.process.Append("\u2023");
    }

    [Macro("ell")]
    internal static void EllipsisMacro(MacroCallInfo info)
    {
        info.process.Append("\u2026");
    }

    [Macro("sec")]
    internal static void SectionMacro(MacroCallInfo info)
    {
        info.process.Append('\u00a7');
    }

    [Macro("deg")]
    internal static void DegreeMacro(MacroCallInfo info)
    {
        info.process.Append('\u00b0');
    }

    [Macro("pm")]
    internal static void DegreePlusMinus(MacroCallInfo info)
    {
        info.process.Append('\u00b1');
    }
    #endregion

    #region Debug Macros
    [Macro("pid")]
    internal static void GetProcessIDMacroShortHand(MacroCallInfo info)
    {
        GetProcessIDMacro(info);
    }
    [Macro("process_id")]
    internal static void GetProcessIDMacro(MacroCallInfo info)
    {
        info.process.Append(info.process.ID.ToString());
    }
    #endregion

    #region Cast Macros
    [Macro("exp")]
    internal static void SetCastExpressionMacroShortHand(MacroCallInfo info, string value)
    {
        SetCastExpressionMacro(info, value);
    }

    [Macro("expression")]
    internal static void SetCastExpressionMacro(MacroCallInfo info, string value)
    {
        if (value.GetType() == typeof(string))
        {
            info.process.ChangeCastExpression(info, value);
            return;
        }
    }

    [Macro("vo")]
    internal static void SetCastVoiceShortHand(MacroCallInfo info, string value)
    {
        SetCastVoice(info, value);
    }

    [Macro("voice")]
    internal static void SetCastVoice(MacroCallInfo info, string value)
    {
        if (value.GetType() == typeof(string))
        {
            info.process.ChangeCastVoice(info, value);
            return;
        }
    }
    #endregion

    #region Scene/Curtain Macros
    [Macro("cue_scene")]
    internal static void SetSceneMacro(MacroCallInfo info, string value)
    {
        info.process.CurrentSceneInfo = new SceneInfo() { name = value };
    }

    #endregion

    #region Variable Control Macros
    [Macro("var")]
    internal static void InitializeVariableMacro(MacroCallInfo info, string identifier, object initialValue)
    {

    }

    [Macro("set")]
    internal static void SetVariableMacro(MacroCallInfo info, string identifier, object newValue)
    {

    }

    [Macro("get")]
    internal static void GetVariableMacro(MacroCallInfo info, string identifier)
    {

    }

    [Macro("test")]
    internal static void TestMacro(MacroCallInfo info, int value)
    {
        Console.WriteLine($"Test Macro Successful: {value}");
    }
    #endregion
}