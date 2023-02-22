using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XVNMLStd.Core.Marcos
{
    public static class MacroManager
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
                // We actually don't want to include this assembly.
                // We want macros outside of this.
                if (assembly == Assembly.GetExecutingAssembly()) continue;

                // TODO: Collect all classes that have MacroLibrary on it.
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
                        Console.WriteLine($"Macro {attribute.macroName} successfully attached to method {method.Name}");
                        continue;
                    }

                    Console.WriteLine($"Invalid Parameter Types for Macro {attribute.macroName}; " +
                        $"Attached method: {method.Name}");
                    continue;
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MacroAttribute : Attribute
    {
        public Type? macroLibraryType;
        public string macroName;
        public MethodInfo? method;
        public Type[] argumentTypes;

        public MacroAttribute(string name, params Type[] argTypes)
        {
            macroName = name;
            argumentTypes = argTypes;
        }

        public void ValidateMethodParameters(out bool result)
        {
            ParameterInfo[]? methodParameterInfo = method!.GetParameters();
            result = methodParameterInfo == null || methodParameterInfo.Count() == 0;

            if (result) return;

            for(int i = 0; i < argumentTypes.Length; i++)
            {
                var type = argumentTypes[i];
                result = type.Equals(methodParameterInfo![i].ParameterType);
                if (result == false) return;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MacroLibraryAttribute : Attribute
    {
        public Type targetType;
        public MacroLibraryAttribute(Type type)
        {
           targetType = type;
        }
    }
}
