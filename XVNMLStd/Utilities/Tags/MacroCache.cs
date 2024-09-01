using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using XVNML.Core.Macros;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;

using static XVNML.ParameterConstants;
using static XVNML.FlagConstants;
using static XVNML.DirectoryConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("macroCache", new[] { typeof(Source), typeof(Proxy) }, TagOccurance.PragmaOnce)]
    public sealed class MacroCache : TagBase
    {
        protected override string[]? AllowedParameters => new[]
        {
            RootScopeParameterString
        };

        protected override string[]? AllowedFlags => new[]
        {
            PassFlagString
        };

        [JsonProperty] private Macro[]? _macros;
        [JsonProperty] private string? _rootScope;

        [JsonProperty]
        public Macro[]? Macros
        {
            get
            {
                if (DefinedMacrosCollection.CachedMacros?.Count != 0) return _macros;
                foreach (var macro in _macros!)
                {
                    DefinedMacrosCollection.AddToMacroCache(
                        (macro.TagName!, macro.parentTag?.TagName),
                        macro.symbol!,
                        new[] { (macro.arg, macro.type) }!,
                        macro.ChildMacros,
                        _rootScope,
                        out _
                    );
                }
                return _macros;
            }
        }

        [JsonProperty]
        private DirectoryRelativity PathMode { get; set; } = DirectoryRelativity.Absolute;

        [JsonProperty]
        private string? Source { get; set; }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);

            if (GetFileAsSource(fileOrigin, OnSourceDOMCreation)) return;

            _rootScope = GetParameterValue<string>(RootScopeParameterString);
            ProcessData();
        }

        private bool GetFileAsSource(string? fileOrigin, Action<XVNMLObj> onDOMCreation)
        {

            PathMode = GetParameterValue<DirectoryRelativity>(PathRelativityParameterString);
            Source = GetParameterValue<string>(SourceParameterString);

            if (Source?.ToLower() == NullParameterString)
            {
                XVNMLLogger.LogWarning($"Macro Cache Source was set to null for {TagName}", this);
                return false;
            }

            if (Source != null)
            {
                string sourcePath = PathMode == DirectoryRelativity.Absolute
                    ? Source
                    : new StringBuilder()
                    .Append(fileOrigin)
                    .Append(DefaultMacroCacheDirectory)
                    .Append(Source)
                    .ToString()!;

                XVNMLObj.Create(sourcePath, onDOMCreation);
                return true;
            }

            return false;
        }

        private void OnSourceDOMCreation(XVNMLObj dom)
        {
            if (dom == null) return;

            var target = dom?.source?.SearchElement<MacroCache>(TagName ?? string.Empty);
            if (target == null) return;

            value = target.value;
            TagName = target.TagName;
            RootScope = dom?.Root?.TagName;

            _rootScope = target.GetParameterValue<string>(RootScopeParameterString);

            ProcessData();
        }

        public void ProcessData()
        {
            _macros = Collect<Macro>();

            if (_rootScope == null) return;

            foreach (var macro in _macros)
            {
                macro.RestrictToScope(_rootScope);
            }
        }

        public Macro? GetMacro(string name) => Macros.First(macro => macro.TagName?.Equals(name) == true);
    }
}
