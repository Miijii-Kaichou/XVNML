using System;
using System.Collections.Concurrent;
using System.Threading;
using XVNML.Core.Dialogue;
using XVNML.Core.Dialogue.Structs;
using XVNML.Utility.Dialogue;
using XVNML.Utility.Macros;

namespace XVNML.Core.Macros
{
    internal static class MacroInvoker
    {
        internal static bool[] RetriesQueued = new bool[0];

        private static ConcurrentQueue<(string, object[], MacroCallInfo)>[]? RetryQueues;

        private static bool[] IsBlocked = new bool[0];

        internal static void Init()
        {
            RetriesQueued = new bool[DialogueWriter.TotalProcesses];
            IsBlocked = new bool[DialogueWriter.TotalProcesses];
            RetryQueues = new ConcurrentQueue<(string, object[], MacroCallInfo)>[DialogueWriter.TotalProcesses];
        }

        internal static void Call(string macroSymbol, object[] args, MacroCallInfo info)
        {
            lock (info.process.processLock)
            {
                if (IsBlocked[info.process.ID])
                {    
                    SendForRetry((macroSymbol, args, info));
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
            lock (value.info.process.processLock)
            {
                if (RetryQueues == null) return;
                RetryQueues[value.info.process.ID] ??= new ConcurrentQueue<(string, object[], MacroCallInfo)>();
                RetryQueues[value.info.process.ID].Enqueue(value);
                UpdateRetryQueuedFlags(value.info.process);
            }
        }

        private static void UpdateRetryQueuedFlags(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (RetryQueues == null) return;
                RetriesQueued[process.ID] = RetryQueues[process.ID].Count > 0;
            }
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
                object? currentArg = args[i];
                Type? requiredArg = targetMacro?.argumentTypes?[i];

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
                for (int i = 0; i < blockInfo.macroCalls.Length;)
                {
                    var (macroSymbol, args) = blockInfo.macroCalls[i];
                    Call(macroSymbol, args, callInfo);
                    i++;
                }
            }
        }

        internal static void AttemptRetries(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (RetryQueues == null) return;
                if (RetryQueues[process.ID] == null) return; 
                if (RetryQueues[process.ID].IsEmpty) return;
                RetryQueues[process.ID].TryDequeue(out var call);
                Call(call.Item1, call.Item2, call.Item3);
                UpdateRetryQueuedFlags(process);
            }
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