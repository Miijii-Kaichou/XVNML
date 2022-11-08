using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("cast", typeof(CastDefinitions), TagOccurance.Multiple)]
    sealed class Cast : TagBase
    {
        PortraitDefinitions? _portraitDefinitions;
        VoiceDefinitions? _voiceDefinitions;

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            _portraitDefinitions    = GetElement<PortraitDefinitions>();
            _voiceDefinitions       = GetElement<VoiceDefinitions>();
        }

        public Portrait GetPortrait(string name) => _portraitDefinitions?[name]!;
        public Voice GetVoice(string name) => _voiceDefinitions?[name]!;
    }
}