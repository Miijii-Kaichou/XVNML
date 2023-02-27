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
        DialogueWriter.AllocateChannels(1);
        finished = false;
        XVNMLObj? name = XVNMLObj.Create("E:\\Documents\\Repositories\\C#\\XVNML\\XVNMLConsoleTest\\TestXVNML.xvnml");
        var dialogue1 = name?.Root?.GetElement<Dialogue>(0)?.dialogueOutput;
        var dialogue2 = name?.Root?.GetElement<Dialogue>(1)?.dialogueOutput;

        DialogueWriter.Write(dialogue1, 0);
        DialogueWriter.OnLineSubstringChange[0] = UpdateText;
        DialogueWriter.OnLinePause[0] = DontReadInput;
        DialogueWriter.OnDialogueFinish[0] = Finish;

        while (finished == false)
        {
            continue;
        }
        DialogueWriter.ShutDown();
        Console.Clear();
        Console.WriteLine("Press any key to end program");
        Console.ReadKey();
        return;
    }

    private static void DontReadInput(DialogueWriterProcessor process)
    {
        DialogueWriter.MoveNextLine(process);
    }

    private static void ReadInput(DialogueWriterProcessor process)
    {
        Console.ReadKey();
        DialogueWriter.MoveNextLine(process);
    }

    private static void Finish(DialogueWriterProcessor process)
    {
        finished = true; 
    }

    private static void UpdateText(DialogueWriterProcessor process)
    {
        outputString = process.DisplayingContent;
        Console.SetCursorPosition(0, 0);
        Console.Out.Write(outputString);
    }
}
