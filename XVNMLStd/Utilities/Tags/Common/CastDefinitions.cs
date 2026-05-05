using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("castDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class CastDefinitions : TagBase
    {
        [JsonProperty] private Cast[]? _castMembers;
        public Cast[]? CastMembers => _castMembers;

        public Cast? this[string name]
        {
            get { return GetCast(name); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _castMembers = Collect<Cast>();
        }

        public Cast? GetCast(string name) => CastMembers.First(cast => cast.TagName?.Equals(name) == true);
    }
}
