using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("source", TagOccurance.PragmaOnce)]
    public sealed class Source : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }
    }
}
