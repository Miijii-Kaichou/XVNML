using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
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
