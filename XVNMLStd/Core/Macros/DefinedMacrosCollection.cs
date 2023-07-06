using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XVNML.Core.Assemblies;
using XVNML.Utility.Macros;

namespace XVNML.Core.Macros
{
    public static class DefinedMacrosCollection
    {
        public static SortedDictionary<string, List<MacroAttribute>>? ValidMacros { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static void ManifestMacros()
        {
            if (IsInitialized) return;

            ValidMacros = new SortedDictionary<string, List<MacroAttribute>>();

            Type[] libraryTypes;

            libraryTypes = DomainAssemblyState.DefinedTypes.Where(c => c.IsClass && c.GetCustomAttribute<MacroLibraryAttribute>() != null).ToArray();
            EstablishLibraries(libraryTypes);

            IsInitialized = true;
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
                var attribute = (MacroAttribute)method.GetCustomAttribute(typeof(MacroAttribute));

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
