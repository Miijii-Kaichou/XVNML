using Newtonsoft.Json;
using System.Linq;
using System.Text;
using XVNML.Core.Enums;
using XVNML.Core.Macros;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;

using static XVNML.ParameterConstants;
using static XVNML.DirectoryConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("macroCache", new[] { typeof(Source), typeof(Proxy) }, TagOccurance.PragmaOnce)]
    public sealed class MacroCache : TagBase
    {
        protected override string[]? AllowedParameters => new[]
        {
            SourceParameterString,
            RootScopeParameterString,
            PathRelativityParameterString
        };

        [JsonProperty] private Macro[]? _macros;
        [JsonProperty] private string? _rootScope;

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

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);

            DirectoryRelativity rel = GetParameterValue<DirectoryRelativity>(PathRelativityParameterString);
            string source = GetParameterValue<string>(SourceParameterString);

            if (source?.ToLower() == NullParameterString)
            {
                XVNMLLogger.LogWarning($"Macro Cache Source was set to null for {TagName}", this);
                return;
            }

            if (source != null)
            {
                string sourcePath = rel == DirectoryRelativity.Absolute
                    ? source
                    : new StringBuilder()
                    .Append(fileOrigin)
                    .Append(DefaultMacroCacheDirectory)
                    .Append(source)
                    .ToString()!;

                XVNMLObj.Create(sourcePath, dom =>
                {
                    if (dom == null) return;

                    var target = dom?.source?.SearchElement<MacroCache>(TagName ?? string.Empty);
                    if (target == null) return;

                    value = target.value;
                    TagName = target.TagName;
                    RootScope = dom?.Root?.TagName;

                    _rootScope = target.GetParameterValue<string>(RootScopeParameterString);

                    ProcessData();
                });
                return;
            }

            _rootScope = GetParameterValue<string>(RootScopeParameterString);
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
