using System;
using System.Linq;
using System.Reflection;
using XVNML.Core.Dialogue;

namespace XVNML.Utility.Macros
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MacroAttribute : Attribute
    {
        public Type? macroLibraryType;
        public string macroName;
        public MethodInfo? method;
        public Type[]? argumentTypes;

        public MacroAttribute(string name)
        {
            macroName = name;
        }

        public void ValidateMethodParameters(out bool result)
        {
            ParameterInfo[]? methodParameterInfo = method!.GetParameters();

            result = methodParameterInfo == null || methodParameterInfo.Count() == 0;
            bool isImplicit = (argumentTypes == null || argumentTypes.Length == 0) &&
                methodParameterInfo.Count() > 0;

            if (isImplicit)
            {
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
}