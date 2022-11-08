using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("source", TagOccurance.PragmaOnce)]
    sealed class Source : TagBase
    {
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
