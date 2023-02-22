// See https://aka.ms/new-console-template for more information


using XVNML.Core.Dialogue;
using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

class Program
{
    // 60 characters a second is the default
    static uint TextRate = 60;
    static void Main(string[] args)
    {
        while (true)
        {
            XVNMLObj? name = XVNMLObj.Create("E:\\Documents\\Repositories\\C#\\XVNML\\XVNMLConsoleTest\\TestXVNML.xvnml");
            var dialogue1 = name?.Root?.GetElement<Dialogue>()?.dialogueOutput?.GetLine(0);
            var dialogue2 = name?.Root?.GetElement<Dialogue>()?.dialogueOutput?.GetLine(1);

            DoTypeWriterEffect(dialogue1);
            Console.ReadKey();
            DoTypeWriterEffect(dialogue2);
            return;
        }
    }

    private static void DoTypeWriterEffect(DialogueLine dialogue)
    {
        bool IsRunning = true;
        int pos = -1;
        while(IsRunning)
        {
            Next();
            if (pos > dialogue.Content.Length - 1)
            {
                IsRunning = false;
                continue;
            }
            dialogue.ReadPosAndExecute(pos);
            var substring = dialogue.Content?[pos];
            Console.Write(substring);
            Thread.Sleep((int)TextRate);
        }
        void Next() => pos++;
    }

    internal static void SetTextRate(uint rate)
    {
        TextRate = rate;
    }
}
