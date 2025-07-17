using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common.Macro.SMLCast
{
    [TagNamespace("m")]
    [AssociateWithTag("expression", typeof(Macro), TagOccurance.Multiple)]
    public sealed class MacroExpression : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            /* This will be a shorthand version of the macro tag.  *
             * It'll input the information for you as if you were  *
             * typing the macro tag inside a XVNML file.           *
             * This also results in cleaner code, and gives the    *
             * tokenizer and parser less to process, which is ideal*
             * for optimization.                                   */

            base.OnResolve(fileOrigin);
        }
    }
}
