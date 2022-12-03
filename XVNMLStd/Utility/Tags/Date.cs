using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("date", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Date : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
