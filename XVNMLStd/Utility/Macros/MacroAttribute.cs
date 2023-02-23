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
            bool isImplicit = (argumentTypes == null || argumentTypes.Length == 0) &&
                methodParameterInfo.Count() > 0;

            if (isImplicit)
            {
                argumentTypes = new Type[methodParameterInfo!.Length];
                for(int i = 0; i < methodParameterInfo.Length; i++)
                    argumentTypes[i] = methodParameterInfo[i].ParameterType;
                result = true;
                return;
            }

            if (result) return;

            for (int i = 0; i < argumentTypes?.Length; i++)
            {
                var type = argumentTypes[i];
                result = type.Equals(methodParameterInfo![i].ParameterType);
                if (result == false) return;
            }
        }
    }
}
