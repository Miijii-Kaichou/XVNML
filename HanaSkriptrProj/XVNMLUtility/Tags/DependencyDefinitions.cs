using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dependencyDefinitions", typeof(Proxy), TagOccurance.PragmaOnce)]
    sealed class DependencyDefinitions : TagBase
    {
        public Dependency[]? Dependencies => Collect<Dependency>();
        public Dependency? this[string name]
        {
            get { return GetDependency(name.ToString()); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        public Dependency? GetDependency(string name) => this[name];
    }
}
