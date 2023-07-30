using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using XVNML.Core.Macros;
using XVNML.Core.Parser;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Tags;


namespace XVNML.Utilities
{
    public class XVNMLObj
    {
        public static XVNMLObj? Instance { get; private set; }
        public FileInfo? FileInfo { get; private set; }

        [JsonIgnore]
        public Action<XVNMLObj>? onDOMCreated;

        [JsonProperty]
        internal Proxy? proxy;

        [JsonProperty]
        internal Source? source;

        [JsonProperty]
        public TagBase? Root
        {
            get
            {
                if (proxy != null) return proxy;
                if (source != null) return source;
                return null;
            }
        }

        [JsonProperty]
        public bool? IsBeingUsedAsSource => source != null;

        [JsonProperty]
        internal TagParser? xvnmlParser = new TagParser();

        private XVNMLObj()
        {
            new XVNMLObj(null);
        }

        private XVNMLObj(TagParser? origin)
        {
            xvnmlParser = origin;
            if (xvnmlParser == null) return;
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

        public static void Create(string fileTarget, Action<XVNMLObj>? onCreation, bool generateCacheFile = false)
        {
            DefinedTagsCollection.ManifestTagTypes();
            DefinedMacrosCollection.ManifestMacros();

            var xvnmlParser = new TagParser();
            xvnmlParser.SetTarget(fileTarget);
            xvnmlParser.Parse(() =>
            {
                Instance = new XVNMLObj(xvnmlParser){FileInfo = new FileInfo(fileTarget)};
                Instance!.onDOMCreated = onCreation;

                if (generateCacheFile) GenerateCache(fileTarget);

                XVNMLLogger.Log($"XVNMLObj [{Instance.FileInfo.Name}] successfully created...", Instance);
                Instance.onDOMCreated?.Invoke(Instance);
            });
        }

        public static void UseOrCreate(string fileTarget, Action<XVNMLObj?> onCreation)
        {
            if (CheckCacheData(fileTarget, out string cachePath))
            {
                DefinedTagsCollection.ManifestTagTypes();
                DefinedMacrosCollection.ManifestMacros();

                var json = File.ReadAllText(cachePath);

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Objects,
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };

                Instance = JsonConvert.DeserializeObject<XVNMLObj>(json, settings);
                Instance!.onDOMCreated = onCreation;
                Instance.onDOMCreated?.Invoke(Instance);
                return;
            }

            Create(fileTarget, dom =>
            {
                Instance = dom;
                onCreation?.Invoke(Instance);
            }, true);
        }

        private static bool CheckCacheData(string fileTarget, out string cachePath)
        {
            string fileName;
            GenerateDataDirectoryFromPath(fileTarget, out fileName, out cachePath);

            var fullCachePath = Path.Combine(cachePath, fileName);

            cachePath = fullCachePath;
            return (File.Exists(fullCachePath));
        }

        private static void GenerateCache(string fileTarget)
        {
            if (Instance == null) return;
            if (Instance.proxy == null) return;

            string fileName, cachePath;
            GenerateDataDirectoryFromPath(fileTarget, out fileName, out cachePath);
            Directory.CreateDirectory(cachePath);

            var fullCachePath = Path.Combine(cachePath, fileName);

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var json = JsonConvert.SerializeObject(Instance, settings);

            File.WriteAllText(fullCachePath, json);
        }

        private static void GenerateDataDirectoryFromPath(string fileTarget, out string fileName, out string cachePath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fileTarget);
            FileInfo fileInfo = new FileInfo(fileTarget);
            var parentDirectory = dInfo.Parent.FullName;
            fileName = fileInfo.Name + ".cache.json";
            cachePath = parentDirectory;
        }
    }
}
