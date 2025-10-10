using Newtonsoft.Json;
using XVNML.Core.Extensions;
using XVNML.Core.Macros;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common.Macros.SMLCast
{
    [AssociateWithTag("macro::expression", typeof(Macro), TagOccurance.Multiple)]
    public sealed class MacroExpression : MacroBase
    {
        [JsonProperty]
        public string Symbol => "expression";

        private (string macroName, string? macroParent) _macroRefKey;

        public override void OnResolve(string? fileOrigin)
        {
            /* This will be a shorthand version of the macro tag.  *
             * It'll input the information for you as if you were  *
             * typing the macro tag inside a XVNML file.           *
             * This also results in cleaner code, and gives the    *
             * tokenizer and parser less to process, which is ideal*
             * for optimization.                                   */

            base.OnResolve(fileOrigin);
            value = GetParameterValue<object?>(nameof(value));
            DefinedMacrosCollection.AddToMacroCache((TagName, parentTag?.TagName)!, Symbol, new[] { (value, value?.DetermineValueType()) }!, null, null, out _macroRefKey);
        }
    }
}
