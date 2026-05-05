using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;
namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("title", typeof(Metadata), TagOccurance.PragmaLocalOnce)]
    public sealed class Title : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            value ??= TagName;
        }
    }
}
