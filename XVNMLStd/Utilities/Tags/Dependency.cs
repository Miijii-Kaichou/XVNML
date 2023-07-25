using XVNML.Core.Tags;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("dependency", typeof(DependencyDefinitions), TagOccurance.Multiple)]
    public sealed class Dependency : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
