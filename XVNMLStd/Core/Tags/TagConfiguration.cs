using System.Dynamic;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Core.Tags
{
    public struct TagConfiguration
    {
        public string LinkedTag { get; internal set; }
        public string[]? DependingTags { get; internal set; }
        public TagOccurance? TagOccurance { get; internal set; }
        public bool UserDefined { get; internal set; }
        public string? FromNamespace { get; internal set; }
    }
}