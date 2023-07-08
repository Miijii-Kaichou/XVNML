using System;
using System.Text;

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

        // I am {bob}
        // input: {bob}
        public static string RemoveFirstOccuranceOf(this string target, string input)
        {
            var _string = target;
            var i = -1;
            int start = 0;
            foreach(var character in _string)
            {
                i++;

                if (character == input[0])
                {
                    start = i;
                    continue;
                }

                if (character == input[^1])
                {
                    int end = i + 1;
                    var match = target[start..end];
                    if (match != input) continue;
                    var resultingString = target.Remove(start, input.Length);
                    return resultingString;
                }
            }

            return target;
        }
    }
}
