using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XVNML.Utility.Macros;

namespace XVNML.Core.Macros
{
    public static class DefinedMacrosCollection
    {
        public static SortedDictionary<string, MacroAttribute>? ValidMacros { get; private set; }

        public static bool IsInitialized { get; private set; }

        public static void ManifestMacros()
        {
            if (IsInitialized) return;

            ValidMacros = new SortedDictionary<string, MacroAttribute>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (Assembly assembly in assemblies)
            {
                Type[] libraryTypes = assembly.GetTypes().Where(c => c.IsClass && c.GetCustomAttribute<MacroLibraryAttribute>() != null).ToArray();

                EstablishLibraries(libraryTypes);
            }

            IsInitialized = true;
        }

        private static void EstablishLibraries(Type[] libraryTypes)
        {
            foreach(Type lib in libraryTypes)
            {
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
                    if (ValidMacros!.ContainsKey(attribute.macroName)) continue;
                    attribute.macroLibraryType = lib;
                    attribute.method = method;
                    attribute.ValidateMethodParameters(out bool result);
                    if (result)
                    {
                        ValidMacros.Add(attribute.macroName, attribute);
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
