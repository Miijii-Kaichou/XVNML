using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("imageDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class ImageDefinitions : TagBase
    {
        [JsonProperty] private Image[]? _images;
        public Image[]? Images => _images;

        public Image? this[string name]
        {
            get { return GetImage(name); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _images = Collect<Image>();
        }

        Image? GetImage(string name) => Images.First(img => img.TagName?.Equals(name) == true);
    }
}