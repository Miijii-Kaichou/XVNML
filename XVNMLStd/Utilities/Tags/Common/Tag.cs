using Newtonsoft.Json;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("tag", typeof(Tags), TagOccurance.Multiple)]
    public sealed class Tag : TagBase
    {
        [JsonProperty] public string? name;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            name = TagName!;
        }
    }
}
