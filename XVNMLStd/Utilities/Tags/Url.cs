using XVNML.Core.Tags;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("url", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Url : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            value ??= TagName;
        }
    }
}
