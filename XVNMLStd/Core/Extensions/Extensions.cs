using System;
using System.Linq;
using System.Text;
using XVNML.Core.Enums;
using XVNML.Core.Lexer;

namespace XVNML.Core.Extensions
{
    public static class Extensions
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
            foreach (var character in _string)
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

        public static string ReplaceFirstOccuranceOf(this string target, string input, string replaceWith)
        {
            var _string = target;
            var i = -1;
            int start = 0;

            foreach (var character in _string)
            {
                i++;

                if (character == input[0])
                {
                    start = i;

                    if (input.Count() > 1) continue;

                    string resultingString = ExtractResultingString(target, input, replaceWith, start);

                    return resultingString;
                }

                if (character == input[^1])
                {
                    int end = i + 1;
                    var match = target[start..end];
                    if (match != input) continue;

                    string resultingString = ExtractResultingString(target, input, replaceWith, start);

                    return resultingString;
                }
            }

            return target;
        }

        private static string ExtractResultingString(string target, string input, string replaceWith, int start)
        {
            return target.Remove(start, input.Length)
                                                            .Insert(start, replaceWith);
        }

        internal static string ReplaceBlockFromPosition(this string input, int position, string replaceWith)
        {
            var content = input;
            var beginNormalization = false;
            StringBuilder stringBuilder = new StringBuilder(2048);
            for (int i = position; i < content?.Length; i++)
            {
                var character = content[i];
                if (character == '{')
                {
                    // Start normalizing
                    stringBuilder.Append(character);
                    beginNormalization = true;
                    continue;
                }

                if (character == '}')
                {
                    stringBuilder.Append(character);
                    beginNormalization = false;

                    var resultingString = stringBuilder.ToString();
                    input = input!.RemoveFirstOccuranceOf(resultingString);
                    input = input!.Insert(position, replaceWith);
                    stringBuilder.Clear();
                    return input;
                }

                if (beginNormalization)
                {
                    stringBuilder.Append(character);
                }
            }

            return input;
        }

        public static Type? DetermineValueType(this object target)
        {
            if (target == null) return null;
            var token = Tokenizer.Tokenize(target.ToString(), TokenizerReadState.Local)![0];

            if (token == null) return target.GetType();

            if (token.Type == TokenType.String || token.Type == TokenType.Identifier)
            {
                if (token.Text?.ToLower() == "true" || token.Text?.ToLower() == "false") return typeof(bool);
                return typeof(string);
            }
                if (token.Type == TokenType.Number)
            {
                Type numberType = token.Value!.GetType();
                return numberType;
            }
            var type = target.GetType();
            return type;
        }
    }
}
