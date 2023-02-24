using System;
using System.Linq;
using XVNML.Core.Dialogue;
using XVNML.Utility.Macros;

namespace XVNML.Core.Macros
{
    internal static class MacroInvoker
    {
        internal static void Call(string macroSymbol, object[] args, MacroCallInfo info)
        {
            if (DefinedMacrosCollection.ValidMacros?.ContainsKey(macroSymbol) == false)
            {
                throw new InvalidMacroException(macroSymbol, info.source.currentLine!);
            }

            var targetMacro = DefinedMacrosCollection.ValidMacros?[macroSymbol];

            args = ResolveMacroArgumentTypes(targetMacro, args);

            object[] finalArgs = FinalizeArgumentData(args, info);

            targetMacro?.method?.Invoke(info, finalArgs);
        }

        private static object[] FinalizeArgumentData(object[] args, MacroCallInfo source)
        {
            var value = source;
            object[] finalArgs = new object[args.Length + 1];

            for (int i = 0; i < finalArgs.Length; i++)
            {
                if (i == 0)
                {
                    finalArgs[i] = value;
                    continue;
                }

                finalArgs[i] = args[i - 1];
            }

            return finalArgs;
        }

        private static object[] ResolveMacroArgumentTypes(MacroAttribute? targetMacro, object[] args)
        {
            if (args == null || args.Length == 0) return Array.Empty<object>();

            for (int i = 0; i < args.Length; i++)
            {
                var currentArg = args[i];
                var requiredArg = targetMacro?.argumentTypes[i];

                // TODO: Convert to whatever type the attribute has
                currentArg = Convert.ChangeType(currentArg, requiredArg);
                args[i] = currentArg;
            }

            return args;
        }

        internal static void Call(this MacroBlockInfo info, MacroCallInfo source)
        {
            foreach ((string macroSymbol, object[] args) call in info.macroCalls)
                Call(call.macroSymbol, call.args, source);
        }
    }
}