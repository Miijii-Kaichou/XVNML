using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
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
        private static ConcurrentQueue<(string, object[], MacroCallInfo)> RetryQueue = new ConcurrentQueue<(string, object[], MacroCallInfo)>();

        private static bool[] IsBlocked = new bool[0];

        internal static void Init()
        {
            IsBlocked = new bool[DialogueWriter.TotalProcesses];
        }

        internal static void Call(string macroSymbol, object[] args, MacroCallInfo info)
        {
            lock (info.process.processLock)
            {
                if (IsBlocked[info.process.ID])
                {
                    SendForRetry((macroSymbol, args, info));
                    Thread.Sleep(10);
                    AttemptRetries();
                    return;
                }


                var targetMacro = DefinedMacrosCollection.ValidMacros?[macroSymbol];

                args = ResolveMacroArgumentTypes(targetMacro, args);

                object[] finalArgs = FinalizeArgumentData(args, info);

                targetMacro?.method?.Invoke(info, finalArgs);
            }
        }

        private static void SendForRetry((string macroSymbol, object[] args, MacroCallInfo info) value)
        {
            RetryQueue.Enqueue(value);
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
            lock (callInfo.process.processLock)
            {
                foreach (var (macroSymbol, args) in blockInfo.macroCalls)
                {
                    Call(macroSymbol, args, callInfo);
                };
            }
        }

        private static void AttemptRetries()
        {
            if (RetryQueue.IsEmpty) return;
            RetryQueue.TryDequeue(out var call);
            Call(call.Item1, call.Item2, call.Item3);
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