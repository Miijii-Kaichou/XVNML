﻿// See https://aka.ms/new-console-template for more information


using XVNML.Core.Dialogue;
using XVNML.Utility.Macros;
using XVNML.Utility.Dialogue;

[MacroLibrary(typeof(MacroTestClass))]
public static class MacroTestClass
{
    [Macro("delay")]
    private static void DelayMacro(DialogueLine src, uint milliseconds)
    {
        Console.WriteLine(src.ToString());
        // Delay macro logic here.
        Thread.Sleep((int)milliseconds);
    }

    [Macro("insert")]
    private static void InsertMacro(DialogueLine src, string text)
    {
        // Insert macro logic here.
        Console.Write(text);
    }

    [Macro("set_text_speed")]
    private static void SetTextSpeed(DialogueLine src, uint level)
    {

        // Speed macro logic here.
        DialogueWriter.SetTextRate(level == 0 ? level : 1000 / level);
    }

    [Macro("clear")]
    private static void ClearText(DialogueLine src)
    {
        Console.Clear();
    }
}