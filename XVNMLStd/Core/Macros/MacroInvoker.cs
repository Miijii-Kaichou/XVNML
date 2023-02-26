using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using XVNML.Core.Dialogue;
using XVNML.Utility.Dialogue;
using XVNML.Utility.Macros;

using Timer = System.Timers.Timer;

namespace XVNML.Core.Macros
{
    internal static class MacroInvoker
    {
        private static ConcurrentQueue<(string symbol, object[] args, MacroCallInfo info)> QueuedForRetry = new ConcurrentQueue<(string symbol, object[] args, MacroCallInfo info)>();

        private static bool[] IsBlocked;

        internal static void Init()
        {
            IsBlocked = new bool[DialogueWriter.TotalProcesses];
        }

        internal static void Call(string macroSymbol, object[] args, MacroCallInfo info)
        {
            while (IsBlocked[info.process.ID])
            {
                continue;
            }

            var targetMacro = DefinedMacrosCollection.ValidMacros?[macroSymbol];

            args = ResolveMacroArgumentTypes(targetMacro, args);


            object[] finalArgs = FinalizeArgumentData(args, info);

            targetMacro?.method?.Invoke(info, finalArgs);

        }

        private static object[] FinalizeArgumentData(object[] args, MacroCallInfo info)
        {
            var value = info;
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

        internal static void Call(this MacroBlockInfo blockInfo, MacroCallInfo callInfo)
        {
            foreach(var info in blockInfo.macroCalls)
            {             
                Call(info.macroSymbol, info.args, callInfo);
            };
        }

        internal static void Block(DialogueWriterProcessor process)
        {
            IsBlocked[process.ID] = true;
        }

        internal static void UnBlock(DialogueWriterProcessor process)
        {
            IsBlocked[process.ID] = false;
        }
    }
}