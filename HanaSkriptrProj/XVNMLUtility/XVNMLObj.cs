using XVNML.Core.Parser;
using XVNML.XVNMLUtility.Tags;

namespace XVNML.XVNMLUtility
{
    public class XVNMLObj
    {
        private static readonly XVNMLObj? Instance;
        internal Proxy? proxy;
        internal Source? source;
        public bool? IsBeingUsedAsSource => source != null;

        XVNMLObj()
        {
            if (Parser.RootTag == null) return;

            var root = Parser.RootTag;

            //Valid root names are "proxy" and "source"
            if (root.GetType() == typeof(Proxy))
            {
                proxy = root as Proxy;
                return;
            }

            if (root.GetType() == typeof(Source))
            {
                source = root as Source;
                return;
            }
        }

        internal static XVNMLObj? Create(ReadOnlySpan<char> fileTarget)
        {

            Parser.SetTarget(fileTarget);
            Parser.Parse();
            return new XVNMLObj();
        }
    }
}
