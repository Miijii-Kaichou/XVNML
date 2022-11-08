using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("copyright", typeof(Metadata), TagOccurance.PragmaOnce)]
    sealed class Copyright : TagBase
    {
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
