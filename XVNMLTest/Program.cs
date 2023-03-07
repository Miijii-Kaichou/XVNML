// See https://aka.ms/new-console-template for more information
using System.Text;
using XVNML.Core.Dialogue;
using XVNML.Utility.Dialogue;
using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

static class Program
{
    static void Main(string[] args)
    {
        XVNMLObj obj = XVNMLObj.Create(@"E:\Documents\Repositories\C#\XVNML\XVNMLTest\XVNMLFiles\test0.main.xvnml");
        if (obj == null) return;
        if (obj.Root == null) return;

        Console.OutputEncoding = Encoding.UTF8;

        DialogueScript script = obj.Root.GetElement<Dialogue>()?.dialogueOutput!;
        
        DialogueWriter.AllocateChannels(1);
        DialogueWriter.OnLineSubstringChange![0] += UpdateConsole;
        DialogueWriter.OnNextLine![0] += ClearConsole;
        DialogueWriter.OnLinePause![0] += MoveNext;
        DialogueWriter.OnDialogueFinish![0] += Finish;
        DialogueWriter.Write(script);
        return;
    }

    private static void MoveNext(DialogueWriterProcessor sender)
    {
        Console.Write("^");
        Console.ReadKey();
        DialogueWriter.MoveNextLine(sender);
    }

    private static void Finish(DialogueWriterProcessor sender)
    {
        DialogueWriter.ShutDown();
        return;
    }

    private static void ClearConsole(DialogueWriterProcessor sender)
    {
        Console.Clear();
    }

    private static void UpdateConsole(DialogueWriterProcessor sender)
    {
        Console.SetCursorPosition(0, 0);
        Console.Write(sender.DisplayingContent);
    }
}