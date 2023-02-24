// See https://aka.ms/new-console-template for more information


using XVNML.Core.Dialogue;
using XVNML.Utility.Macros;
using XVNML.Utility.Dialogue;
using System.Threading;

[MacroLibrary(typeof(StandardMacroLibrary))]
internal static class StandardMacroLibrary
{
    [Macro("delay")]
    internal static void DelayMacro(MacroCallInfo info, uint milliseconds)
    {
        // Delay macro logic here.
        info.source.Wait(milliseconds);
    }

    [Macro("insert")]
    internal static void InsertMacro(MacroCallInfo info, string text)
    {
        // Insert macro logic here.
        info.source.Append(text);
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
    internal static void CurlyBracketEnd(MacroCallInfo info)
    {
        info.source.Append('}');
    }

    [Macro("pause")]
    internal static void PauseMacro(MacroCallInfo info)
    {
        info.source.WasControlledPause = true;
        info.source.waitingForUserInput = true;
    }
}