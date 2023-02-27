using XVNML.Core.TagParser;
using XVNML.Core.Tags;
using XVNML.XVNMLUtility.Tags;
using XVNML.Core.Macros;

namespace XVNML.XVNMLUtility
{
    public class XVNMLObj
    {
        public static XVNMLObj? Instance { get; private set; }
        internal Proxy? proxy;
        internal Source? source;

        public TagBase? Root
        {
            get
            {
                if (proxy != null) return proxy as TagBase;
                if (source != null) return source as TagBase;
                return null;
            }
        }

        public bool IsBeingUsedAsSource => source != null;

        private readonly Parser xvnmlParser = new Parser();
        private XVNMLObj(Parser origin)
        {
            xvnmlParser = origin;
            if (xvnmlParser._rootTag == null) return;

            var root = xvnmlParser._rootTag;

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

        public static XVNMLObj? Create(string fileTarget)
        {
            DefinedTagsCollection.ManifestTagTypes();
            DefinedMacrosCollection.ManifestMacros();
            var xvnmlParser = new Parser();
            xvnmlParser.SetTarget(fileTarget);
            xvnmlParser.Parse();
            Instance = new XVNMLObj(xvnmlParser);
            return Instance;
        }
    }
}
