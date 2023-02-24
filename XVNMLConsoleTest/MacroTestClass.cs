// See https://aka.ms/new-console-template for more information


using XVNML.Core.Dialogue;
using XVNML.Utility.Macros;
using XVNML.Utility.Dialogue;

[MacroLibrary(typeof(MacroTestClass))]
internal static class MacroTestClass
{
    [Macro("delay")]
    internal static void DelayMacro(DialogueWriterProcessor source, uint milliseconds)
    {
        // Delay macro logic here.
        Thread.Sleep((int)milliseconds);
    }

    [Macro("insert")]
    internal static void InsertMacro(DialogueWriterProcessor source, string text)
    {
        // Insert macro logic here.
        source.Append(text);
    }

    [Macro("set_text_speed")]
    internal static void SetTextSpeed(DialogueWriterProcessor source, uint level)
    {
        // Speed macro logic here.
        source.SetProcessRate(level == 0 ? level : 1000 / level);
    }

    [Macro("clear")]
    internal static void ClearText(DialogueWriterProcessor source)
    {
        Console.Clear();
    }

    [Macro("new_line")]
    internal static void NewLineMacro(DialogueWriterProcessor source)
    {
        source.Append('\n');
    }

    [Macro("paren")]
    internal static void ParenthesisMacro(DialogueWriterProcessor source)
    {
        source.Append('(');
    }

    [Macro("paren_end")]
    internal static void EndParenthesisMacro(DialogueWriterProcessor source)
    {
        source.Append(')');
    }
}