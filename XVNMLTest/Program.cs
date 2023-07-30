// See https://aka.ms/new-console-template for more information
using System.Text;
using XVNML.Core.Dialogue;
using XVNML.Utilities.Dialogue;
using XVNML.Utilities;
using XVNML.Utilities.Tags;
using System.Text.RegularExpressions;

static partial class Program
{
    private static bool finished = false;

    private static XVNMLObj MainDom = null;

    private static string[] DialogueList = 
    {
        "MainTest",
        "Osaka's Dream"
    };

    static void Main(string[] args)
    {
        XVNMLObj.Create(@"../../../XVNMLFiles/consoleApp.main.xvnml", dom =>
        {
            if (dom == null) return;
            if (dom.Root == null) return;

            MainDom = dom;

            Console.OutputEncoding = Encoding.UTF8;
           
        });

        PrintMainMenu();

        while (finished == false)
            continue;
    }

    private static void PrintMainMenu()
    {
        Console.Clear();
        Console.WriteLine("Type in which dialogue you want to run...");

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < DialogueList.Length+1; i++)
        {
            var entryString = sb.Append(i)
                .Append(')')
                .Append(' ')
                .Append(i > DialogueList.Length - 1 ? "Close Console Application..." : DialogueList[i]);
            Console.WriteLine(entryString);
            sb.Clear();
        }

        var selectedDialogue = Console.ReadLine();

        RunDialogue(selectedDialogue);
    }

    private static void RunDialogue(string selectedDialogue)
    {
        var rule = MyRegex();
        ValidateRule(selectedDialogue, rule, delegate() { ExecuteDialogue(Convert.ToInt32(selectedDialogue)); }, PrintMainMenu);
    }

    private static void ExecuteDialogue(int dialogueIndex)
    {
        if (dialogueIndex == 2)
        {
            finished = true;
            return;
        }

        Console.Clear();

        DialogueScript script = MainDom.Root?.GetElement<Dialogue>(DialogueList[dialogueIndex])?.dialogueOutput!;

        DialogueWriter.AllocateChannels(1);
        DialogueWriter.OnPrompt![0] += DisplayPrompts;
        DialogueWriter.OnPromptResonse![0] += RespondToPrompt;
        DialogueWriter.OnLineSubstringChange![0] += UpdateConsole;
        DialogueWriter.OnNextLine![0] += ClearConsole;
        DialogueWriter.OnLinePause![0] += MoveNext;
        DialogueWriter.OnDialogueFinish![0] += Finish;
        DialogueWriter.Write(script);
    }

    private static void ValidateRule(string input, Regex regexRule, Action success, Action failure)
    {
        Action determinedAction = regexRule.IsMatch(input) ? success : failure;
        determinedAction?.Invoke();
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

        DialogueWriter.OnPrompt![0] -= DisplayPrompts;
        DialogueWriter.OnPromptResonse![0] -= RespondToPrompt;
        DialogueWriter.OnLineSubstringChange![0] -= UpdateConsole;
        DialogueWriter.OnNextLine![0] -= ClearConsole;
        DialogueWriter.OnLinePause![0] -= MoveNext;
        DialogueWriter.OnDialogueFinish![0] -= Finish;

        PrintMainMenu();
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

    [GeneratedRegex("^[0-9]+?")]
    private static partial Regex MyRegex();
}