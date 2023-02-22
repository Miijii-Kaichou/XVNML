using System;
using System.Collections.Generic;
using System.Text;
using XVNML.Core.Dialogue;
using XVNMLStd.Utility.Macros;

namespace XVNMLStd.Core.Macros
{
    internal static class MacroInvoker
    {
        internal static void Call(string macroSymbol, object[] args, MacroBlockInfo source)
        {
            if(DefinedMacrosCollection.ValidMacros?.ContainsKey(macroSymbol) == false)
            {
                throw new InvalidMacroException(macroSymbol);
            }

            var targetMacro = DefinedMacrosCollection.ValidMacros?[macroSymbol];
            args = ResolveMacroArgumentTypes(targetMacro, args);

            targetMacro?.method?.Invoke(source, args);
        }

        private static object[] ResolveMacroArgumentTypes(MacroAttribute? targetMacro, object[] args)
        {
            if(args == null || args.Length == 0) return Array.Empty<object>();

            for(int i = 0; i < args.Length; i++)
            {
                var currentArg = args[i];
                var requiredArg = targetMacro?.argumentTypes[i];

                // TODO: Convert to whatever type the attribute has
                currentArg = Convert.ChangeType(currentArg, requiredArg);
                args[i] = currentArg;
            }

            return args;
        }

        internal static void Call(this MacroBlockInfo info)
        {
            foreach ((string macroSymbol, object[] args) call in info.macroCalls)
                Call(call.macroSymbol, call.args, info);
        }
    }
}