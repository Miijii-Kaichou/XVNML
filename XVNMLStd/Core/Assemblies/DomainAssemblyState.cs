using System;
using System.Collections.Generic;
using System.Reflection;
using XVNML.Utility.Diagnostics;

namespace XVNML.Core.Assemblies
{
    internal static class DomainAssemblyState
    {
        internal static Assembly[]? DomainAssemblies;
        internal static List<Type>? DefinedTypes;

        static DomainAssemblyState()
        {
            Initialize();
        }

        private static void Initialize()
        {
            DomainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            DefinedTypes = new List<Type>();
            foreach (var asm in DomainAssemblies)
            {
                PopulateDefinedTypes(asm);
            }
        }

        private static void PopulateDefinedTypes(Assembly asm)
        {
            var definedTypes = asm.GetTypes();
            foreach (var type in definedTypes)
            {
                DefinedTypes?.Add(type);
            }
        }
    }
}
