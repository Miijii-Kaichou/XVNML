using System;
using XVNML.Core.Macros;
using XVNML.Core.Parser;
using XVNML.Core.Tags;
using XVNML.XVNMLUtility.Tags;

namespace XVNML.XVNMLUtility
{
    public class XVNMLObj
    {
        public static XVNMLObj? Instance { get; private set; }

        public Action<XVNMLObj>? onDOMCreated;

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

        private readonly TagParser xvnmlParser = new TagParser();
        private XVNMLObj(TagParser origin)
        {
            xvnmlParser = origin;
            if (xvnmlParser.root == null) return;

            var root = xvnmlParser.root;

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

        public static void Create(string fileTarget, Action<XVNMLObj>? onCreation)
        {
            DefinedTagsCollection.ManifestTagTypes();
            DefinedMacrosCollection.ManifestMacros();

            var xvnmlParser = new TagParser();
            xvnmlParser.SetTarget(fileTarget);
            xvnmlParser.Parse(() =>
            {
                Instance = new XVNMLObj(xvnmlParser);
                Instance!.onDOMCreated = onCreation;
                Instance.onDOMCreated?.Invoke(Instance);
            });
        }
    }
}
