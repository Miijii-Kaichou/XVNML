// See https://aka.ms/new-console-template for more information


using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            XVNMLObj? name = XVNMLObj.Create("E:\\Documents\\Repositories\\C#\\XVNML\\XVNMLConsoleTest\\TestXVNML.xvnml");
            string dialogue = name?.Root?.GetElement<Dialogue>()?.dialogueOutput?.GetLine(0).Content!;
            Console.WriteLine(dialogue);
            Console.ReadKey();
            return;
        }
    } 
}
