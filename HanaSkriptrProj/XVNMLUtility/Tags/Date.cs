using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("date", typeof(Metadata), TagOccurance.PragmaOnce)]
    sealed class Date : TagBase
    {
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
