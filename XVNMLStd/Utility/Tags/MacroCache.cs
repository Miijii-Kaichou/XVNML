using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Macros;
using XVNML.Core.Tags;

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
                if (DefinedMacrosCollection.CachedMacros?.Count == 0)
                {
                    foreach(var macro in _macros!)
                    {
                        DefinedMacrosCollection.AddToMacroCache((macro.TagName!,macro.parentTag?.TagName), macro.symbol!, new[] { (macro.arg, macro.type) }, macro.ChildMacros);
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
}
