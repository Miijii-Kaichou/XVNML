using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("portraitDefinitions", typeof(Cast), TagOccurance.PragmaLocalOnce)]
    public sealed class PortraitDefinitions : TagBase
    {
        [JsonProperty] private Portrait[]? _portraits;
        public Portrait[]? Portraits =>  _portraits;

        public Portrait? this[string name]
        {
            get { return GetPortrait(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _portraits = Collect<Portrait>();
        }

        Portrait? GetPortrait(string name) => Portraits.First(portrait => portrait.TagName?.Equals(name) == true);
    }
}
