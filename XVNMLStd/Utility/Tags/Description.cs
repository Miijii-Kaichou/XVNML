using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("description", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Description : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
