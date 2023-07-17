// See https://aka.ms/new-console-template for more information
using System.Text;
using XVNML.Core.Dialogue;
using XVNML.Utilities.Dialogue;
using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

static class Program
{
    private static bool finished = false;
    static void Main(string[] args)
    {
        XVNMLObj.Create(@"../../../XVNMLFiles/consoleApp.main.xvnml", dom =>
        {
            if (dom == null) return;
            if (dom.Root == null) return;

            Console.OutputEncoding = Encoding.UTF8;

            DialogueScript script = dom.Root.GetElement<Dialogue>("MainTest")?.dialogueOutput!;

            DialogueWriter.AllocateChannels(1);
            DialogueWriter.OnPrompt![0] += DisplayPrompts;
            DialogueWriter.OnPromptResonse![0] += RespondToPrompt;
            DialogueWriter.OnLineSubstringChange![0] += UpdateConsole;
            DialogueWriter.OnNextLine![0] += ClearConsole;
            DialogueWriter.OnLinePause![0] += MoveNext;
            DialogueWriter.OnDialogueFinish![0] += Finish;
            DialogueWriter.Write(script);
            Console.Clear();
        });

        while (finished == false)
            continue;
    }

    private static void DisplayPrompts(DialogueWriterProcessor sender)
    {
        // We need to somehow graph the current line's prompt answers,
        // as well as where the process to hope to in response.
        var prompts = sender.FetchPrompts();
        Console.Write('\n');
        List<string> responses = prompts!.Keys.ToList();
        for(int i = 0; i < responses.Count; i++)
        {
            var promptData = responses[i];
            Console.Write($"{i}) {promptData}\n");
        }

        var response = Console.ReadLine();
        var responseIndex = Convert.ToInt32(response);
        if (responseIndex > responses.Count - 1) return;
        sender.JumpToStartingLineFromResponse(responses[Convert.ToInt32(response)]);
    }

    private static void RespondToPrompt(DialogueWriterProcessor sender)
    {
        // This is the point where we take the answer of our prompt
        // and tell the process to hope to a specified index of DialogueLines that it has.
        // This luckily will increment as normal, because jumping to a dialogue line is already
        // simple with predetermined indexes to go to.
        ClearConsole(sender);
    }

    private static void MoveNext(DialogueWriterProcessor sender)
    {
        if (sender.IsPass)
        {
            DialogueWriter.MoveNextLine(sender);
            return;
        }

        Console.Write("^");
        Console.ReadKey();
        DialogueWriter.MoveNextLine(sender);
    }

    private static void Finish(DialogueWriterProcessor sender)
    {
        DialogueWriter.ShutDown();
        finished = true;
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