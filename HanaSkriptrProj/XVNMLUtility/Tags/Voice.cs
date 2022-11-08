using XVNML.Core.Parser;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("voice", typeof(VoiceDefinitions), TagOccurance.Multiple)]
    sealed class Voice : TagBase
    {
        internal Audio? audio;
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            var audioRef = parameterInfo?.GetParameter("audio");
            if (audioRef != null && audioRef.isReferencing)
            {
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                Parser.QueueForReferenceSolve(OnAudioReferenceSolve);
                return;
            }

            throw new Exception("\"audio\" parameter must pass a reference");
        }

        void OnAudioReferenceSolve()
        {
            TagBase? source = null;
            TagBase? target = null;
            try
            {
                //Iterate through until you find the right source target;
                source = Parser.RootTag?.elements?.Where(tag => tag.GetType() == typeof(ImageDefinitions)).First();
                target = source?.GetElement<Audio>(parameterInfo?.GetParameter("audio").value?.ToString()!);
                audio = (Audio)Convert.ChangeType(target, typeof(Audio))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {parameterInfo?.GetParameter("img").value?.ToString()!}" +
                    $"audio {source.tagTypeName}");
            }
        }
    }
}
