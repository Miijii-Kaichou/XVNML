using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using XVNML.Core.Macros;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("macroCache", new[] {typeof(Proxy), typeof(Source)}, TagOccurance.PragmaOnce )]
    public sealed class MacroCache : TagBase
    {
        [JsonProperty] private Macro[]? _macros;
        public Macro[]? Macros
        {
            get
            {
                if (DefinedMacrosCollection.CachedMacros.Count == 0)
                {
                    foreach(var macro in _macros)
                    {
                        DefinedMacrosCollection.AddToMacroCache(macro.TagName!, macro.symbol!, macro.arg, macro.type);
                    }
                }
                return _macros;
            }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _macros = Collect<Macro>();
        }

        public Macro? GetMacro(string name) => Macros.First(macro => macro.TagName?.Equals(name) == true);
    }

    [AssociateWithTag("macro", typeof(MacroCache), TagOccurance.Multiple)]
    public sealed class Macro : TagBase
    {
        public object arg;
        public string? symbol;
        public Type type;

        private SortedDictionary<string, Type> _typeMapping = new SortedDictionary<string, Type>
        {
            {"int", typeof(int) },
            {"string", typeof(string) },
            {"bool", typeof(bool) },
            {"uint", typeof(uint) },
            {"float", typeof(uint) },
            {"obj", typeof(object) },
            {"long", typeof(long) }
        };

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ArgParameterString,
                SymbolParameterString,
            };

            AllowedFlags = new[]
            {
                UIntFlagString,
                IntFlagString,
                BoolFlagString,
                StringFlagString,
                LongFlagString,
                FloatFlagString,
                ObjectFlagString
            };

            base.OnResolve(fileOrigin);

            arg = GetParameterValue<object>(ArgParameterString);
            symbol = GetParameterValue<string>(SymbolParameterString);
            DetermineType();

            DefinedMacrosCollection.AddToMacroCache(TagName!, symbol, arg, type);
        }

        private void DetermineType()
        {
            if (HasFlag(UIntFlagString))
            {
                type = _typeMapping[UIntFlagString];
                return;
            }

            if (HasFlag(IntFlagString))
            {
                type = _typeMapping[IntFlagString];
                return;
            }

            if (HasFlag(BoolFlagString))
            {
                type = _typeMapping[BoolFlagString];
                return;
            }

            if (HasFlag(StringFlagString))
            {
                type = _typeMapping[StringFlagString];
                return;
            }

            if (HasFlag(FloatFlagString))
            {
                type = _typeMapping[FloatFlagString];

                return;
            }

            if (HasFlag(LongFlagString))
            {
                type = _typeMapping[LongFlagString];
                return;
            }

            if (HasFlag(ObjectFlagString))
            {
                type = _typeMapping[ObjectFlagString];
                return;
            }
        }
    }
}
