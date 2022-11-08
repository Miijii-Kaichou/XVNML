using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("author", typeof(Metadata), TagOccurance.PragmaOnce)]
    sealed class Author : TagBase
    {
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
