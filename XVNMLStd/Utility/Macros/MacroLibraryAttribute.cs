using System;

namespace XVNML.Utility.Macros
{
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
