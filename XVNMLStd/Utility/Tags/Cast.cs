using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("cast", new[] { typeof(Source), typeof(CastDefinitions) }, TagOccurance.Multiple)]
    public sealed class Cast : TagBase
    {
        const string _CastDir = @"\Casts\";

        PortraitDefinitions? _portraitDefinitions;
        VoiceDefinitions? _voiceDefinitions;

        public Portrait[]? Portraits => _portraitDefinitions?.Portraits;
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
                    var target = dom?.source?.GetElement<CastDefinitions>()!.GetCast(TagName ?? string.Empty) ??
                               dom?.source?.GetElement<CastDefinitions>()!.GetElement<Cast>();
                    if (target == null) return;
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