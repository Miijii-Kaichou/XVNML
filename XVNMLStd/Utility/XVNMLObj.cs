using System;
using XVNML.Core.TagParser;
using XVNML.Core.Tags;
using XVNML.XVNMLUtility.Tags;

namespace XVNML.XVNMLUtility
{
    public class XVNMLObj
    {
        private static readonly XVNMLObj? Instance;
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
        Parser xvnmlParser = new Parser();
        XVNMLObj(Parser origin)
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
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            var xvnmlParser = new Parser();
            xvnmlParser.SetTarget(fileTarget);
            xvnmlParser.Parse();
            return new XVNMLObj(xvnmlParser);
        }
    }
}
