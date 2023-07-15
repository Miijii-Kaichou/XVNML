using System;

namespace XVNML.Utilities.Macros
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
