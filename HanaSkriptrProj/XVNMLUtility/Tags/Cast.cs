using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("cast", typeof(CastDefinitions), TagOccurance.Multiple)]
    public class Cast : TagBase
    {
        PortraitDefinitions _portraitDefinitions;
        VoiceDefinitions _voiceDefinitions;


        public override void OnResolve()
        {
            base.OnResolve();
           _portraitDefinitions = GetElement<PortraitDefinitions>();
            _voiceDefinitions = GetElement<VoiceDefinitions>();
        }

        public Portrait GetPortrait(string name) => _portraitDefinitions.GetElement<Portrait>(name);
        public Voice GetVoice(string name) => _voiceDefinitions.GetElement<Voice>(name);
    }
}
