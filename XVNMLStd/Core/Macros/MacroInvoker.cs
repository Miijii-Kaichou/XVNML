using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using XVNML.Core.Dialogue;
using XVNML.Core.Dialogue.Structs;
using XVNML.Core.Native;

using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Dialogue;
using XVNML.Utilities.Macros;

namespace XVNML.Core.Macros
{
    internal static class MacroInvoker
    {
        internal static bool[] RetriesQueued = new bool[0];

        private static ConcurrentQueue<(string, (object, Type)[], MacroCallInfo, bool, string?)>[]? RetryQueues;

        private static bool[] IsBlocked = new bool[0];
        private static int _allocatedProcesses = -1;

        internal static void Init()
        {
            if (DialogueWriter.TotalProcesses == _allocatedProcesses) return;
            RetriesQueued = new bool[DialogueWriter.TotalProcesses];
            IsBlocked = new bool[DialogueWriter.TotalProcesses];
            RetryQueues = new ConcurrentQueue<(string, (object, Type)[], MacroCallInfo, bool, string?)>[DialogueWriter.TotalProcesses];
        }

        internal static void Call(string macroSymbol, (object, Type)[] args, MacroCallInfo info, bool isRef = false, string? parent = null)
        {
            lock (info.process.processLock)
            {
                if (IsBlocked[info.process.ID])
                {
                    SendForRetry((macroSymbol, args, info, isRef, parent));
                    return;
                }

                if (isRef)
                {
                    Call(macroSymbol, parent, info, 0);
                    return;
                }

                var targetMacros = DefinedMacrosCollection.ValidMacros?[macroSymbol];

                args = ResolveMacroArgumentTypes(targetMacros!, args, out MacroAttribute? correctMacro);

                object[] finalArgs = FinalizeArgumentData(args, info);
                correctMacro?.method?.Invoke(info, finalArgs);
            }
        }

        private static void SendForRetry((string macroSymbol, (object, Type)[] args, MacroCallInfo info, bool isRef, string?) retryInfo)
        {
            lock (retryInfo.info.process.processLock)
            {
                if (RetryQueues == null) return;

                RetryQueues[retryInfo.info.process.ID] ??= new ConcurrentQueue<(string, (object, Type)[], MacroCallInfo, bool, string?)>();
                RetryQueues[retryInfo.info.process.ID].Enqueue(retryInfo);

                UpdateRetryQueuedFlags(retryInfo.info.process);
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

            if (args == null || args.Length == 0 || args[0].Item1 == null)
                return Array.Empty<(object, Type)>();

            int pos = 0;

            var argTypes = args
                .Select(a =>
                {
                    if (a.Item2 == typeof(object) &&  RuntimeReferenceTable.Map.ContainsKey(a.Item1.ToString()))
                    {
                        var value = RuntimeReferenceTable.Get(a.Item1.ToString());

                        a.Item1 = value.value!;
                        a.Item2 = value.type;
                        args[pos] = value!;
                    }
                    pos++;
                    return a.Item2;
                })
                .ToArray();

            var targetMacro = targetMacros.Count < 2 ?
                targetMacros[0] :
                targetMacros.Where(m =>
                {
                    if (m!.argumentTypes?.Length == 0 && argTypes.Length != 0)
                        return false;

                    for (int i = 0; i < m!.argumentTypes?.Length; i++)
                    {
                        var type = m.argumentTypes[i];

                        if (m.argumentTypes.Length != argTypes.Length)
                            return false;
                        if (type == typeof(int) && argTypes[i] == typeof(uint))
                            continue;
                        if (type != typeof(object) && ReferenceEquals(type, argTypes[i]) == false)
                            return false;
                    }

                    return true;
                }).FirstOrDefault();

            correctMacro = targetMacro;

            for (int i = 0; i < args.Length; i++)
            {
                object? currentArg = args[i].Item1;
                Type? requiredArg = targetMacro?.argumentTypes?[i];

                object? opposingArg = args[i].Item2;

                if (args[i].Item2 == typeof(uint) && requiredArg == typeof(int))
                {
                    // TODO: Convert to whatever type the attribute has
                    args[i].Item1 = Convert.ChangeType(currentArg, requiredArg);
                    continue;
                }

                if (ReferenceEquals(requiredArg, args[i].Item2) == false && requiredArg != typeof(object))
                {
                    CheckForArgValidation(args, targetMacro, i, requiredArg);
                }

                currentArg = args[i].Item1;

                // TODO: Convert to whatever type the attribute has
                args[i].Item1 = Convert.ChangeType(currentArg, requiredArg);
            }

            return args;
        }

        private static void CheckForArgValidation((object, Type)[] args, MacroAttribute? targetMacro, int i, Type? requiredArg)
        {
            var invalid = false;

            if (args[i].Item2 == typeof(object))
            {
                invalid = RuntimeReferenceTable.Map.ContainsKey(args[i].Item1.ToString());
                
                var value = RuntimeReferenceTable.Get(args[i].Item1.ToString());
                invalid = !ReferenceEquals(requiredArg, requiredArg == typeof(string) ? requiredArg : value.type);

                args[i] = (value.value, requiredArg)!;
            }

            if (invalid)
            {
                throw new Exception($"Argument {i} for the macro \"{targetMacro?.macroName}\"" +
                    $" requires a value of type {requiredArg?.Name}.\n" +
                    $"The Value passed into Argument {i} is a(n) {args[i].Item2.Name}");
            }
        }

        internal static void Call(this MacroBlockInfo blockInfo, MacroCallInfo callInfo)
        {
            lock (callInfo.process.processLock)
            {
                for (int i = 0; i < blockInfo.macroCalls.Length;)
                {
                    var (macroSymbol, args, isRef, parent) = blockInfo.macroCalls[i];
                    Call(macroSymbol, args, callInfo, isRef, parent);
                    i++;
                }
            }
        }

        internal static void Call(string macroName, string? parent, MacroCallInfo info, int index)
        {
            string? macroParent = parent ?? DefinedMacrosCollection.GetParentOf(macroName);
            string? macroRealName = macroName == "macro" ? DefinedMacrosCollection.GetRealNameFromParent(macroParent, index) : macroName;
            var data = DefinedMacrosCollection.CachedMacros?[(macroRealName, macroParent)];
            var callIndex = info.process.cursorIndex;
            var callInfo = new MacroCallInfo() { callIndex = callIndex, process = info.process, callScope = info.callScope };

            if (string.IsNullOrEmpty(data?.rootScope) == false
            && data?.rootScope?.Equals(callInfo!.callScope) == false &&
            callInfo!.callScope != null)
            {
                string msg = $"Call Inconsistency! Call Scope does not match Root Scope: {macroName} ({data?.rootScope}) calling in ({callInfo!.callScope}.)";
                XVNMLLogger.LogError(msg, callInfo, data);
                return;
            }

            if (data?.children != null && data?.children.Length > 0)
            {
                int i = 0;
                foreach (var macro in data?.children!)
                {
                    Call(macro.TagName!, macro.parentTag?.TagName, callInfo, i++);
                }
                return;
            }

            (string macroSymbol, (object, Type)[] argData, bool isRef, string? parent) call = (data?.symbol, data?.argData, false, parent)!;
            Call(new MacroBlockInfo() { blockPosition = callIndex, macroCalls = new[] { call } }, callInfo);
        }


        internal static void AttemptRetries(DialogueWriterProcessor process)
        {
            lock (process.processLock)
            {
                if (RetryQueues == null) 
                    return;
                if (RetryQueues[process.ID] == null) 
                    return;
                if (RetryQueues[process.ID].IsEmpty) 
                    return;

                RetryQueues[process.ID].TryDequeue(out var call);

                Call(call.Item1, call.Item2, call.Item3, call.Item4, call.Item5);
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