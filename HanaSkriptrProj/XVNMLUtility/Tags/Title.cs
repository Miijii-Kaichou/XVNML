using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("title", typeof(Metadata), TagOccurance.PragmaLocalOnce)]
    sealed class Title : TagBase
    {
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }
    }
}
