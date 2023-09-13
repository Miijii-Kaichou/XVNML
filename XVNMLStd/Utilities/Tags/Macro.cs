﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using XVNML.Core.Extensions;
using XVNML.Core.Macros;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Tags;
using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("macro", new[] { typeof(MacroCache), typeof(Macro) }, TagOccurance.Multiple)]
    public sealed class Macro : TagBase
    {
        [JsonProperty] public object? arg;
        [JsonProperty] public string? symbol;
        [JsonProperty] public Type? type;

        [JsonProperty] private readonly string? _rootScope;
        public string? RootScope => _rootScope;

        [JsonProperty] private Macro[]? _childMacros;
        public Macro[]? ChildMacros => _childMacros;

        [JsonProperty] private Arg[]? _macroArguments;
        public Arg[]? MacroArguments => _macroArguments;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ArgParameterString,
                SymbolParameterString,
            };

            base.OnResolve(fileOrigin);

            arg = GetParameterValue<object>(ArgParameterString);
            symbol = GetParameterValue<string>(SymbolParameterString);

            _macroArguments = Collect<Arg>();

            _childMacros = Collect<Macro>();

            if (_macroArguments?.Length > 0)
            {
                // Create ArgDataSets
                List<(object value, Type type)> argDataSet = new List<(object value, Type type)>();
                foreach(var arg in _macroArguments) argDataSet.Add((arg.ArgData.Value, arg.ArgData.Type));
                DefinedMacrosCollection.AddToMacroCache((TagName!,parentTag?.TagName), symbol, argDataSet.ToArray(), null);
                return;
            }

            DefinedMacrosCollection.AddToMacroCache((TagName, parentTag?.TagName)!, symbol, new[] { (arg, arg.DetermineValueType()) }!, _childMacros);
        }
    }
}
