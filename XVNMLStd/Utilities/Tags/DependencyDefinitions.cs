using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dependencyDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class DependencyDefinitions : TagBase
    {
        [JsonProperty] private Dependency[]? _dependencies;
        public Dependency[]? Dependencies => _dependencies;

        public Dependency? this[string name]
        {
            get { return GetDependency(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _dependencies = Collect<Dependency>();
        }

        public Dependency? GetDependency(string name) => Dependencies.First(dependency => dependency.TagName?.Equals(name) == true);
    }
}
