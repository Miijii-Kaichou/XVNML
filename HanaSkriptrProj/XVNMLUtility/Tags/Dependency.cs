using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dependency", typeof(DependencyDefinitions), TagOccurance.Multiple)]
    sealed class Dependency : TagBase
    {
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
