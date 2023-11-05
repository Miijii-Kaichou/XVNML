using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using XVNML.Core.Extensions;

namespace XVNML.Core.Native
{
    public static class RuntimeReferenceTable
    {
        internal static SortedDictionary<string, (object? value, Type type)> Map = new SortedDictionary<string, (object?, Type)>();

        public static void ProcessVariableExpression(string input, Action<object?> onSuccess, Action onFail)
        {
            if (Regex.IsMatch(input, @";[A-Za-z0-9_-]+;"))
            {
                var identifier = input.Split(';', StringSplitOptions.RemoveEmptyEntries)[0];
                var variable = Get(identifier);
                onSuccess(variable.value);
                return;
            }

            onFail();
        }

        internal static void Declare([CallerMemberName] string identifier = "", Type initialType = default!)
        {
            var value = initialType == typeof(int) ? (int)default :
                        initialType == typeof(string) ? string.Empty :
                        initialType == typeof(float) ? (float)default :
                        initialType == typeof(double) ? (double)default :
                        initialType == typeof(uint) ? (uint)default :
                        Activator.CreateInstance(initialType);

            Map.Add(identifier, (value, initialType));
        }

        public static void Set([CallerMemberName] string identifier = "", object? value = null)
        {
            Type? valueType = value?.DetermineValueType();

            if (Map.ContainsKey(identifier) == false)
            {
                Declare(identifier, valueType!);
            }

            var (_, type) = Map[identifier];

            if (type != valueType) return;

            Map[identifier] = (value, type);
        }

        public static (object? value, Type type) Get([CallerMemberName] string identifier = "")
        {
            if (Map.ContainsKey(identifier) == false) return (identifier, typeof(string));
            return Map[identifier];
        }
    }
}
