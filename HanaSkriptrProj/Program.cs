// See https://aka.ms/new-console-template for more information
using XVNML.Core.Tags;
using XVNML.XVNMLUtility;
using XVNML.XVNMLUtility.Tags;

namespace XVNML
{
    public static class EntryPoint
    {
        public static string TargetFile = @"C:\Users\tclte\Desktop\TestXVNML.xvnml";
        static bool Active = false;
        static int Main(string[] args)
        {
            Active = true;

            DefinedTagsCollection.ManifestTagTypes();

            //Create XVNML File
            XVNMLObj xvnml = XVNMLObj.Create(TargetFile);
            var metadata = xvnml.proxy.GetElement<Metadata>();
            while (Active)
            {
                Console.WriteLine(metadata.title);
                Console.ReadKey();
                Active = false;
            }
            return 0;
        }
    }
}