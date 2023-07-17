using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XVNML.Core.Assemblies;
using XVNML.Utilities.Macros;
using XVNML.XVNMLUtility.Tags;

namespace XVNML.Core.Macros
{
    public static class DefinedMacrosCollection
    {
        public static bool IsInitialized { get; private set; }
        
        public static SortedDictionary<string, List<MacroAttribute>>? ValidMacros { get; private set; }
        public static SortedDictionary<(string, string?), (string symbol, (object arg, Type type)[] argData, Macro[] children)>? CachedMacros { get; private set; }

        public static void ManifestMacros()
        {
            if (IsInitialized) return;

            ValidMacros = new SortedDictionary<string, List<MacroAttribute>>();
            CachedMacros = new SortedDictionary<(string, string?), (string symbol, (object arg, Type type)[] argData, Macro[] children)>();

            Type[] libraryTypes;

            libraryTypes = DomainAssemblyState
                .DefinedTypes
                .Where(c => c.IsClass && c.GetCustomAttribute<MacroLibraryAttribute>() != null)
                .ToArray();

            EstablishLibraries(libraryTypes);

            IsInitialized = true;
        }

        internal static void AddToMacroCache((string macroName, string? macroParent) macroRef, string validSymbol, (object arg, Type type)[] argDataSet, Macro[]? children)
        {
            if (CachedMacros?.ContainsKey(macroRef) == true)
            {
                var countOfExistingMacro = CachedMacros.Where(t => t.Key.Item1.Contains(macroRef.macroName) && t.Key.Item1.Contains("[unnamed]") && t.Key.Item2 == macroRef.macroParent).Count();
                macroRef.macroName = $"{macroRef.macroName}({countOfExistingMacro})[unnamed]";
            };
            CachedMacros!.Add(macroRef, (validSymbol, argDataSet, children)!);
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
    }
}
