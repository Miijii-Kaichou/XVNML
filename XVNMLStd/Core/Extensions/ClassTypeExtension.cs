using System;

namespace XVNML.Core.Extensions
{
    internal static class ClassTypeExtension
    {
        public static string[]? Names(this Type[] types)
        {
            if (types == null || types[0] == null) return null;

            var names = new string[types.Length];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = types[i].Name;
            }

            return names;
        }

        public static void Up(this ref int value)
        {
            value++;
        }
    }
}
