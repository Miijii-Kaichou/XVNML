using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using XVNML.Core.Assemblies;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Macros;
using XVNML.Utilities.Tags.Common;

namespace XVNML.Core.Macros
{
    public struct InternalMacroCache
    {
        [JsonProperty]
        private CachedMarcoData[] _data;

        internal InternalMacroCache(SortedDictionary<(string, string?), (string symbol, (object arg, Type type)[] argData, Macro[] children, string? rootScope)>? data)
        {
            int size = data!.Count;

            List<CachedMarcoData> _cachedList = new List<CachedMarcoData>(size);

            // Proceed to generate CacheMacroData information...
            for(int i = 0; i < size; i++)
            {
                CachedMarcoData newData = new CachedMarcoData(data.ElementAt(i));
                _cachedList.Add(newData);
            }

            _data = _cachedList.ToArray();
        }

        internal CachedMarcoData[] Get() => _data;
    }

    public class CachedMarcoData
    {
        [JsonProperty] public string macroName      = string.Empty;
        [JsonProperty] public string? macroParent   = string.Empty;

        [JsonProperty] public string symbol         = string.Empty;
        [JsonProperty] public ArgData[] argData     = Array.Empty<ArgData>();
        [JsonProperty] public Macro[] children      = Array.Empty<Macro>();
        [JsonProperty] public string? rootScope     = string.Empty;

        public CachedMarcoData(KeyValuePair<(string, string?), (string symbol, (object arg, Type type)[] argData, Macro[] children, string? rootScope)>? pair) 
        {
            if (pair == null) return;

            macroName = pair.Value.Key.Item1;
            macroParent = pair.Value.Key.Item2;

            symbol = pair.Value.Value.symbol;

            argData = new ArgData[pair.Value.Value.argData.Length];
            int _argIncrement = 0;
            foreach(var data in  pair.Value.Value.argData)
            {
                argData[_argIncrement++] = new ArgData(data.arg, data.type);
            }

            children = pair.Value.Value.children;
            rootScope = pair.Value.Value.rootScope;
        }
    }

    public struct ArgData
    {
        public object arg;
        public Type type;

        public ArgData(object a, Type t)
        {
            arg = a;
            type = t;
        }
    }

    public static class DefinedMacrosCollection
    {
        public static bool IsInitialized { get; private set; }
        
        public static SortedDictionary<string, List<MacroAttribute>>? ValidMacros { get; private set; }
        public static SortedDictionary<(string, string?), (string symbol, (object arg, Type type)[] argData, Macro[] children, string? rootScope)>? CachedMacros { get; private set; }

        public static void ManifestMacros()
        {
            if (IsInitialized) return;

            ValidMacros = new SortedDictionary<string, List<MacroAttribute>>();
            CachedMacros = new SortedDictionary<(string, string?), (string symbol, (object arg, Type type)[] argData, Macro[] children, string? rootScope)>();

            Type[] libraryTypes;

            libraryTypes = DomainAssemblyState
                .DefinedTypes
                .Where(c => c.IsClass && c.GetCustomAttribute<MacroLibraryAttribute>() != null)
                .ToArray();

            EstablishLibraries(libraryTypes);

            IsInitialized = true;
        }

        internal static void AddToMacroCache((string macroName, string? macroParent) macroRef, string validSymbol, (object arg, Type type)[] argDataSet, Macro[]? children, string? rootScope, out (string macroName, string? macroParent) macroRefKey)
        {
            if (CachedMacros?.ContainsKey(macroRef) == true)
            {
                var countOfExistingMacro = CachedMacros.Where(t => t.Key.Item1.Contains(macroRef.macroName) && t.Key.Item1.Contains("[unnamed]") && t.Key.Item2 == macroRef.macroParent).Count();
                macroRef.macroName = $"{macroRef.macroName}({countOfExistingMacro})[unnamed]";
            };

            macroRefKey = macroRef;
            CachedMacros!.Add(macroRef, (validSymbol, argDataSet, children, rootScope)!);
        }

        internal static string? GetParentOf(string macroName)
        {
            return CachedMacros.Where(t => t.Key.Item1 == macroName).FirstOrDefault().Key.Item2;
        }

        internal static string GetRealNameFromParent(string macroParent, int index)
        {
            return CachedMacros.Where(t => t.Key.Item2 == macroParent).ToArray()[index].Key.Item1;
        }

        private static void EstablishLibraries(Type[] libraryTypes)
        {
            for (int i = 0; i < libraryTypes.Length; i++)
            {
                Type lib = libraryTypes[i];
                var methods = lib.GetRuntimeMethods();

                ProcessMethods(lib, methods);
            }
        }

        private static void ProcessMethods(Type lib, IEnumerable<MethodInfo> methods)
        {
            foreach (MethodInfo method in methods)
            {
                var attributes = (MacroAttribute[])method.GetCustomAttributes(typeof(MacroAttribute));

                RegisterAttributes(attributes, method, lib);         
            }
        }

        private static void RegisterAttributes(MacroAttribute[] attributes, MethodInfo method, Type lib)
        {
            foreach(var attribute in attributes)
            {
                if (attribute != null)
                {
                    attribute.macroLibraryType = lib;
                    attribute.method = method;
                    attribute.ValidateMethodParameters(out bool result);

                    if (ValidMacros!.ContainsKey(attribute.macroName))
                    {
                        ValidMacros[attribute.macroName].Add(attribute);
                        continue;
                    }

                    if (result)
                    {
                        ValidMacros.Add(attribute.macroName, new List<MacroAttribute>() { attribute });
                        continue;
                    }

                    Console.WriteLine($"Invalid Parameter Types for Macro {attribute.macroName}; " +
                        $"Attached method: {method.Name}");

                    continue;
                }
            }
        }

        public static void CacheCollectionState(string fileTarget, string? destinationPath = null)
        {
            string fileName, cachePath;
            GenerateDataDirectoryFromPath(fileTarget, out fileName, out cachePath, destinationPath);
            Directory.CreateDirectory(cachePath);

            string fullCachePath = Path.Combine(cachePath, fileName);

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
                },
            };

            InternalMacroCache data = new InternalMacroCache(CachedMacros);

            var json = JsonConvert.SerializeObject(data, settings);

            File.WriteAllText(fullCachePath, json);
        }

        public static void LoadCollectionCache(string fileTarget, string? destinationPath = null)
        {
            if (CheckCacheData(fileTarget, out string cachePath, destinationPath))
            {
                var json = File.ReadAllText(cachePath);

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Objects,
                    NullValueHandling = NullValueHandling.Include,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Formatting = Formatting.Indented
                };

                InternalMacroCache data = JsonConvert.DeserializeObject<InternalMacroCache>(json, settings);
                ReassignCachedMacros(data);
            }
        }

        private static void ReassignCachedMacros(InternalMacroCache data)
        {
            var internalCacheList = data.Get();

            foreach(var cache in  internalCacheList)
            {
                CachedMacros?.Add((cache.macroName, cache.macroParent), (cache.symbol, GenerateArgDataToTuple(cache.argData), cache.children, cache.rootScope));
            }

            (object, Type)[] GenerateArgDataToTuple(ArgData[] data)
            {
                List<(object arg, Type type)> dataList = new List<(object arg, Type type)>(data.Length);
                foreach(var arg in data)
                {
                    dataList.Add((arg.arg, arg.type));
                }

                return dataList.ToArray();
            }
        }

        private static bool CheckCacheData(string fileTarget, out string cachePath, string? destinationPath = null)
        {
            string fileName;
            GenerateDataDirectoryFromPath(fileTarget, out fileName, out cachePath, destinationPath);

            var fullCachePath = Path.Combine(cachePath, fileName);

            cachePath = fullCachePath;

            return File.Exists(fullCachePath);
        }

        private static void GenerateDataDirectoryFromPath(string fileTarget, out string fileName, out string cachePath, string? destinationPath = null)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fileTarget);
            FileInfo fileInfo = new FileInfo(fileTarget);
            var parentDirectory = destinationPath == null ? dInfo.Parent.FullName : destinationPath;
            fileName = fileInfo.Name + ".cache.dmc.json";
            cachePath = parentDirectory;
        }
    }
}
