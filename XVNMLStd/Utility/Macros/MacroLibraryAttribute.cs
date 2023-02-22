using System;

namespace XVNMLStd.Utility.Macros
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
