// See https://aka.ms/new-console-template for more information


using XVNMLStd.Utility.Macros;

[MacroLibrary(typeof(MacroTestClass))]
public static class MacroTestClass
{
    [Macro("delay", typeof(uint))]
    private static void DelayMacro(uint milliseconds)
    {
        // Delay macro logic here.
        Thread.Sleep((int)milliseconds);
    }

    [Macro("insert", typeof(string))]
    private static void InsertMacro(string text)
    {
        // Insert macro logic here.
        Console.Write(text);
    }

    [Macro("set_text_speed", typeof(uint))]
    private static void SetTextSpeed(uint level)
    {

        // Speed macro logic here.
        Program.SetTextRate(level == 0 ? level : 1000 / level);
    }
}