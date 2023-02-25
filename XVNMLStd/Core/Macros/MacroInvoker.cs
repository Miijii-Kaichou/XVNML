using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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

        static MacroInvoker()
        {
            Timer timer = new Timer(10);
            timer.Elapsed += CallRetries;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private static void CallRetries(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (QueuedForRetry.IsEmpty) return;
            QueuedForRetry.TryDequeue(out var call);
            Call(call.symbol, call.args, call.info);
        }

        internal static void Call(string macroSymbol, object[] args, MacroCallInfo info)
        {
            var targetMacro = DefinedMacrosCollection.ValidMacros?[macroSymbol];

            args = ResolveMacroArgumentTypes(targetMacro, args);

            object[] finalArgs = FinalizeArgumentData(args, info);

            targetMacro?.method?.Invoke(info, finalArgs);
        }

        private static void SendForRetry((string, object[], MacroCallInfo) retryData)
        {
            QueuedForRetry.Enqueue(retryData);
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
            foreach ((string macroSymbol, object[] args) in info.macroCalls)
            {
                Call(macroSymbol, args, source);
            }
        }
    }
}