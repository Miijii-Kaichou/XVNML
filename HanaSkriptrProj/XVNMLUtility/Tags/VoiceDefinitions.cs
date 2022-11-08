﻿using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("voiceDefinitions", typeof(Cast), TagOccurance.PragmaLocalOnce)]
    sealed class VoiceDefinitions : TagBase
    {
        public Voice[]? Voices => Collect<Voice>();
        public Voice? this[string name]
        {
            get { return GetVoice(name.ToString()); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Voice? GetVoice(string name) => this[name];
    }
}