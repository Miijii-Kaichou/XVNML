using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dependencyDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class DependencyDefinitions : TagBase
    {
        public Dependency[]? Dependencies => Collect<Dependency>();
        public Dependency? this[string name]
        {
            get { return GetDependency(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        public Dependency? GetDependency(string name) => Dependencies.First(dependency => dependency.TagName?.Equals(name) == true);
    }
}
