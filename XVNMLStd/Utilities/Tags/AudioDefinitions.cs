using Newtonsoft.Json;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("audioDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class AudioDefinitions : TagBase
    {
        [JsonProperty] private Audio[]? _audio;
        public Audio[]? AudioCollection => _audio;

        public Audio? this[string name]
        {
            get { return GetAudio(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _audio = Collect<Audio>();
        }

        Audio? GetAudio(string name) => AudioCollection.First(audio => audio.TagName?.Equals(name) == true);
    }
}
