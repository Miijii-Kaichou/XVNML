using Newtonsoft.Json;
using System;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;

using static XVNML.Constants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("voice", typeof(VoiceDefinitions), TagOccurance.Multiple)]
    public sealed class Voice : TagBase
    {
        [JsonProperty] public Audio? audioTarget;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                AudioParameterString
            };

            base.OnResolve(fileOrigin);

            TagParameter? audioRef = GetParameter(AudioParameterString);

            if (audioRef != null && audioRef.isReferencing)
            {
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                ParserRef!.QueueForReferenceSolve(OnAudioReferenceSolve);
                return;
            }

            throw new Exception("\"audio\" parameter must pass a reference");
        }

        void OnAudioReferenceSolve()
        {
            TagBase? audioDefinitions = null;
            TagBase? target = null;
            string audio = GetParameterValue<string>(AudioParameterString);
            try
            {
                if (audio.ToLower() == NullParameterString)
                {
                    XVNMLLogger.LogWarning($"Audio Source was set to null for: {TagName}", this);
                    return;
                }

                audioDefinitions = ParserRef!.root?.GetElement<AudioDefinitions>();
                target = audioDefinitions?.GetElement<Audio>(audio?.ToString()!);
                audioTarget = (Audio)Convert.ChangeType(target, typeof(Audio))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {audio}" +
                    $"audio {audioDefinitions!.tagTypeName}");
            }
        }
    }
}
