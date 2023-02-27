// See https://aka.ms/new-console-template for more information
using XVNML.XVNMLUtility;

static class Program
{
    static void Main(string[] args)
    {
        XVNMLObj obj = XVNMLObj.Create(@"E:\Documents\Repositories\C#\XVNML\XVNMLTest\XVNMLFiles\test0.main.xvnml");
        Console.ReadKey();
        return;
    }
}