using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("voiceDefinitions", typeof(Cast), TagOccurance.PragmaLocalOnce)]
    public sealed class VoiceDefinitions : TagBase
    {
        [JsonProperty] private Voice[]? _voices;
        public Voice[]? Voices => _voices;

        public Voice? this[string name]
        {
            get { return GetVoice(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _voices = Collect<Voice>();
        }

        Voice? GetVoice(string name) => Voices.First(voice => voice.TagName?.Equals(name) == true);
    }
}