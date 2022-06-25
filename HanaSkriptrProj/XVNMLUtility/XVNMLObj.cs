using XVNML.Core.Tags;
using XVNML.Core.Parser;
using XVNML.XVNMLUtility.Tags;

namespace XVNML.XVNMLUtility
{
    public class XVNMLObj
    {
        private static XVNMLObj Instance => new XVNMLObj();
        public Proxy proxy;
        public Source source;
        public bool IsBeingUsedAsSource => source != null;

        internal Proxy test;

        XVNMLObj()
        {
            if (Parser.RootTag == null) return;

            var root = Parser.RootTag;

            //Valid root names are "proxy" and "source"
            if(root.GetType() == typeof(Proxy))
            {
                proxy = Parser.RootTag as Proxy;
                return;
            }

            if(root.GetType() == typeof(Source))
            {
                source = Parser.RootTag as Source;
                return;
            }
        }

        internal static XVNMLObj Create(ReadOnlySpan<char> fileTarget)
        {
            Parser.SetTarget(fileTarget);
            Parser.Parse();
            return Instance;
        }
    }
}
