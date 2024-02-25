using System;
using System.Reflection;
using XVNML.Core.Macros;

namespace XVNML.Utilities.Macros
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MacroAttribute : Attribute
    {
        public Type? macroLibraryType;
        public string macroName;
        public MethodInfo? method;
        public Type[]? argumentTypes;

        internal bool isOverride = false;

        public MacroAttribute(string name)
        {
            macroName = name;
            isOverride = DefinedMacrosCollection.ValidMacros!.ContainsKey(macroName);
        }

        public void ValidateMethodParameters(out bool result)
        {
            ParameterInfo[]? methodParameterInfo = method!.GetParameters();
            argumentTypes = new Type[methodParameterInfo!.Length - 1];

            for (int i = 0; i < methodParameterInfo.Length; i++)
            {
                var argType = methodParameterInfo[i].ParameterType;

                // Check if the first type if of DialogueLine type. If not
                // invalidate the macro.
                if (argType == typeof(MacroCallInfo)) continue;
                result = !(i == 0);
                if (i == 0) return;
                argumentTypes[i - 1] = methodParameterInfo[i].ParameterType;
            }
            result = true;
            return;
        }
    }
}