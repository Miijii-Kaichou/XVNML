using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("author", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Author : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            value ??= TagName;
        }
    }
}
