// See https://aka.ms/new-console-template for more information


using XVNMLStd.Core.Marcos;

[MacroLibrary(typeof(MacroTestClass))]
public static class MacroTestClass
{
    [Macro("delay", typeof(int))]
    private static void DelayMacro(int milliseconds)
    {
        // Delay macro logic here.
        Console.WriteLine($"Delaying for {milliseconds} milliseconds");
    }

    [Macro("insert", typeof(string))]
    private static void InsertMacro(string text)
    {
        // Insert macro logic here.
        Console.Write($"Inserting {text}...");
    }

    [Macro("speed", typeof(int))]
    private static void SpeedMacro(int level)
    {
        // Speed macro logic here.
        switch (level)
        {
            case 0: Console.WriteLine("Text at normal speed"); break;
            case 1: Console.WriteLine("Text at slow speed"); break;
            case 2: Console.WriteLine("Text at fast speed"); break;
            case 3: Console.WriteLine("Text at faster speed"); break;
        }
    }

    [Macro("shake", typeof(float), typeof(int))]
    private static void ShakeMacro(float magnitude, int milliseconds)
    {
        // Shake (Camera Shake) logic here
        Console.WriteLine($"Camera Shaking " +
            $"with Magnitude {magnitude} " +
            $"for {milliseconds} milliseconds");
    }

    [Macro("music", typeof(string), typeof(bool), typeof(int))]
    private static void MusicMacro(string musicName, bool pause, bool loop)
    {

    }

    [Macro("audio", typeof(string), typeof(bool))]
    private static void AudioMacro(string audioName, bool oneShot)
    {

    }

    [Macro("portrait", typeof(string))]
    private static void PortraitMacro(string portraitName)
    {

    }
}