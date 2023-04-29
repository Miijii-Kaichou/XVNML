using System;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("voice", typeof(VoiceDefinitions), TagOccurance.Multiple)]
    public sealed class Voice : TagBase
    {
        public Audio? audioTarget;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[] 
            { 
                "audio" 
            };

            base.OnResolve(fileOrigin);

            var audioRef = GetParameter("audio");
            if (audioRef != null && audioRef.isReferencing)
            {
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                parserRef!.QueueForReferenceSolve(OnAudioReferenceSolve);
                return;
            }

            throw new Exception("\"audio\" parameter must pass a reference");
        }

        void OnAudioReferenceSolve()
        {
            TagBase? source = null;
            TagBase? target = null;
            var audio = GetParameterValue("audio");
            try
            {
                if (audio?.ToString().ToLower() == "nil") return;
                //Iterate through until you find the right source target;
                source = parserRef!._rootTag?.elements?
                    .Where(tag => tag.GetType() == typeof(VoiceDefinitions))
                    .First();

                target = source?.GetElement<Audio>(audio?.ToString()!);
                audioTarget = (Audio)Convert.ChangeType(target, typeof(Audio))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {audio}" +
                    $"audio {source!.tagTypeName}");
            }
        }
    }
}
