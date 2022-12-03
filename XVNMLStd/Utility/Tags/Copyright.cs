using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("copyright", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Copyright : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
