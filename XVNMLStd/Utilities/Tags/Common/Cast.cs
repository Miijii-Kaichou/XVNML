using Newtonsoft.Json;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Object;

using static XVNML.ParameterConstants;
using static XVNML.DirectoryConstants;
using XVNML.Core.Tags.Attributes;


namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("cast", new[] { typeof(Source), typeof(CastDefinitions) }, TagOccurance.Multiple)]
    public sealed class Cast : TagBase
    {
        const string _CastDir = DefaultCastDirectory;

        [JsonProperty] PortraitDefinitions? _portraitDefinitions;
        [JsonProperty] VoiceDefinitions? _voiceDefinitions;

        [JsonIgnore]
        public Portrait[]? Portraits => _portraitDefinitions?.Portraits;

        [JsonIgnore]
        public Voice[]? Voices => _voiceDefinitions?.Voices;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                SourceParameterString
            };

            base.OnResolve(fileOrigin);

            var source = GetParameterValue<string>(SourceParameterString);

            if (source?.ToLower() == NullParameterString)
            {
                XVNMLLogger.LogWarning($"Cast Source was set to null for: {TagName}", this);
                return;
            }

            // Check if a source has been specified.
            if (source != null)
            {
                XVNMLObj.Create(fileOrigin + _CastDir + source!.ToString(), dom =>
                {
                    if (dom == null) return;
                    var target = dom?.source?.SearchElement<Cast>(TagName ?? string.Empty);

                    if (target == null) return;

                    RootScope = dom?.Root?.TagName;

                    _portraitDefinitions = target!._portraitDefinitions;
                    _voiceDefinitions = target!._voiceDefinitions;

                });

                return;
            }

            _portraitDefinitions = GetElement<PortraitDefinitions>();
            _voiceDefinitions = GetElement<VoiceDefinitions>();
        }

        public Portrait GetPortrait(string name) => _portraitDefinitions?[name]!;
        public Voice GetVoice(string name) => _voiceDefinitions?[name]!;
    }
}