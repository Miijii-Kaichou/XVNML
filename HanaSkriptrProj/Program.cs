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
            XVNMLObj xvnml = XVNMLObj.Create(@"C:\Users\tclte\Desktop\TestXVNML.xvnml");
            if (xvnml.proxy == null) return -1;
            var metadata = xvnml.proxy.GetElement<Metadata>();
            var castDefinitions = xvnml.proxy.GetElement<CastDefinitions>();
            while (Active)
            {
                Console.WriteLine(metadata.author["name"]);
                Console.WriteLine(metadata.title["name"]);
                Console.ReadKey();
                Active = false;
            }
            return 0;
        }
    }
}