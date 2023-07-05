using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using XVNML.Core.Dialogue;
using XVNML.Core.Dialogue.Structs;
using XVNML.Utility.Dialogue;
using XVNML.Utility.Macros;

namespace XVNML.Core.Macros
{
    internal static class MacroInvoker
    {
        internal static bool[] RetriesQueued = new bool[0];

        private static ConcurrentQueue<(string, (object, Type)[], MacroCallInfo)>[]? RetryQueues;

        private static bool[] IsBlocked = new bool[0];

        internal static void Init()
        {
            RetriesQueued = new bool[DialogueWriter.TotalProcesses];
            IsBlocked = new bool[DialogueWriter.TotalProcesses];
            RetryQueues = new ConcurrentQueue<(string, (object, Type)[], MacroCallInfo)>[DialogueWriter.TotalProcesses];
        }

        internal static void Call(string macroSymbol, (object, Type)[] args, MacroCallInfo info)
        {
            lock (info.process.processLock)
            {
                if (IsBlocked[info.process.ID])
                {
                    SendForRetry((macroSymbol, args, info));
                    return;
                }

                var targetMacros = DefinedMacrosCollection.ValidMacros?[macroSymbol];

                args = ResolveMacroArgumentTypes(targetMacros!, args, out MacroAttribute? correctMacro);

                object[] finalArgs = FinalizeArgumentData(args, info);

                correctMacro?.method?.Invoke(info, finalArgs);
            }
        }

        private static void SendForRetry((string macroSymbol, (object, Type)[] args, MacroCallInfo info) value)
        {
            lock (value.info.process.processLock)
            {
                if (RetryQueues == null) return;

                RetryQueues[value.info.process.ID] ??= new ConcurrentQueue<(string, (object, Type)[], MacroCallInfo)>();
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

        private static object[] FinalizeArgumentData((object, Type)[] args, MacroCallInfo info)
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

                finalArgs[i] = args[i - 1].Item1;
            }

            return finalArgs;
        }

        private static (object, Type)[] ResolveMacroArgumentTypes(List<MacroAttribute?> targetMacros, (object, Type)[] args, out MacroAttribute? correctMacro)
        {
            correctMacro = targetMacros[0];

            if (args == null || args.Length == 0) return Array.Empty<(object, Type)>();

            var argTypes = args.Select(a => a.Item2).ToArray();
            var targetMacro = targetMacros.Count < 2 ? targetMacros[0] : targetMacros.Where(m =>
            {
                for (int i = 0; i < m!.argumentTypes?.Length; i++)
                {
                    var type = m.argumentTypes[i];
                    
                    if (m.argumentTypes.Length != argTypes.Length) return false;
                    if (type == typeof(int) && argTypes[i] == typeof(uint)) continue;
                    if (ReferenceEquals(type, argTypes[i]) == false) return false;
                }

                return true;
            }).FirstOrDefault();

            correctMacro = targetMacro;

            for (int i = 0; i < args.Length; i++)
            {
                object? currentArg = args[i].Item1;
                Type? requiredArg = targetMacro?.argumentTypes?[i];

                if (args[i].Item2 == typeof(uint) && requiredArg == typeof(int))
                {
                    // TODO: Convert to whatever type the attribute has
                    args[i].Item1 = Convert.ChangeType(currentArg, requiredArg);
                    continue;
                }

                if (ReferenceEquals(requiredArg, args[i].Item2) == false && requiredArg != typeof(object))
                {
                    throw new Exception($"Argument {i} for the macro \"{targetMacro?.macroName}\"" +
                        $" requires a value of type {requiredArg?.Name}.\n" +
                        $"The Value passed into Argument {i} is a(n) {args[i].Item2.Name}");
                }

                // TODO: Convert to whatever type the attribute has
                args[i].Item1 = Convert.ChangeType(currentArg, requiredArg);
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