using XVNML.Core.Tags;

namespace XVNML.Utilities.Tags
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
