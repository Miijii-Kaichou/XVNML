// See https://aka.ms/new-console-template for more information


using XVNML.Core.Dialogue;
using XVNML.Utility.Dialogue;
using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

class Program
{
    // 60 characters a second is the default
    static string outputString = string.Empty;
    private static bool finished;

    static void Main(string[] args)
    {
        finished = false;
        XVNMLObj? name = XVNMLObj.Create("E:\\Documents\\Repositories\\C#\\XVNML\\XVNMLConsoleTest\\TestXVNML.xvnml");
        var dialogue1 = name?.Root?.GetElement<Dialogue>(0)?.dialogueOutput;
        var dialogue2 = name?.Root?.GetElement<Dialogue>(1)?.dialogueOutput;

        DialogueWriter.Run(dialogue1);
        DialogueWriter.OnLineSubstringChange = UpdateText;
        DialogueWriter.OnLineFinished = ReadInput;
        DialogueWriter.OnDialogueFinish = Finish;
        while (finished == false)
        {
            continue;
        }

        Console.WriteLine("Press any key to end program");
        Console.ReadKey();
        return;
    }

    private static void ReadInput(DialogueLine sender)
    {
        Console.ReadKey();
        DialogueWriter.MoveNextLine();
    }

    private static void Finish(DialogueLine sender)
    {
        finished = true;
        Console.Clear();
    }

    private static void UpdateText(DialogueLine sender)
    {
        var span = outputString.AsSpan();
        span.Feed();
        outputString = span.ToString();
        Console.Write(outputString);
    }
}
