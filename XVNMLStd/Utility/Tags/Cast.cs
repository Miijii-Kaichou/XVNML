﻿using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("cast", new[] { typeof(Source), typeof(CastDefinitions) }, TagOccurance.Multiple)]
    public sealed class Cast : TagBase
    {
        const string _CastDir = @"\Cast\";
        PortraitDefinitions? _portraitDefinitions;
        VoiceDefinitions? _voiceDefinitions;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);

            // Check if a source has been specified.
            if (parameterInfo?["src"] != null)
            {
                var xvnml = XVNMLObj.Create(fileOrigin + _CastDir + parameterInfo?["src"]!.ToString());
                if (xvnml == null) return;
                var target = xvnml?.source?.GetElement<Cast>(tagName ?? string.Empty) ??
                           xvnml?.source?.GetElement<Cast>();
                if (target == null) return;
                _portraitDefinitions = target!._portraitDefinitions;
                _voiceDefinitions = target!._voiceDefinitions;
                return;
            }

            _portraitDefinitions = GetElement<PortraitDefinitions>();
            _voiceDefinitions = GetElement<VoiceDefinitions>();
        }

        public Portrait GetPortrait(string name) => _portraitDefinitions?[name]!;
        public Voice GetVoice(string name) => _voiceDefinitions?[name]!;
    }
}