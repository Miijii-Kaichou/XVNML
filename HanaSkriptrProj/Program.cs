// See https://aka.ms/new-console-template for more information
using XVNML.Core.Tags;
using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

namespace XVNML
{
    public static class EntryPoint
    {
        static bool Active = false;
        static int Main(string[] args)
        {
            Active = true;

            DefinedTagsCollection.ManifestTagTypes();

            //Create XVNML File
            XVNMLObj xvnml = XVNMLObj.Create(@"C:\Users\tclte\Desktop\TestXVNML\TestXVNML.xvnml")!;
            XVNMLObj testXVNML = XVNMLObj.Create(@"C:\Users\tclte\Desktop\TestXVNML\AllPreBuildTags.xvnml")!;
            XVNMLObj flommcsXVNML = XVNMLObj.Create(@"C:\Users\tclte\Desktop\TestXVNML\Tomakunihahaji.xvnml")!;
            XVNMLObj testStory = XVNMLObj.Create(@"C:\Users\tclte\Desktop\TestXVNML\TestStory.xvnml")!;

            var metadata = xvnml.proxy?.GetElement<Metadata>();
            var dialogueGroup = xvnml.proxy?.GetElement<DialogueGroup>("XVNML Tutorial Basics");
            var RavenRoute = flommcsXVNML.proxy?.GetElement<DialogueGroup>("RavenRoute_EN");
            var prologue = testStory.proxy?.GetElement<DialogueGroup>("Prologue");

            while (Active)
            {
                Console.WriteLine(dialogueGroup!["Chapter 1"]);
                Console.WriteLine(RavenRoute![0]);
                Console.WriteLine(prologue![0]);
                Console.ReadKey();
                Active = false;
            }
            return 0;
        }
    }
}