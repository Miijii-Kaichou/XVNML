using Newtonsoft.Json;
using System;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("scene", typeof(SceneDefinitions), TagOccurance.Multiple)]
    public sealed class Scene : TagBase
    {
        [JsonProperty] public Image? imageTarget;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                SourceParameterString,
                ImageParameterString
            };

            base.OnResolve(fileOrigin);

            // Will evaluation a source first if one exists
            var source = GetParameterValue<string>(AllowedParameters[0]);

            TagParameter? imgRef = null;

            if (source != null)
            {
                if (source.ToLower() == NullParameterString)
                {
                    XVNMLLogger.LogWarning($"Scene Source was set to null for: {TagName}", this);
                    return;
                }

                XVNMLObj.Create(fileOrigin + DefaultSceneDirectory + source.ToString(), dom =>
                {
                    if (dom == null) return;

                    var target = dom?.source?.GetElement<SceneDefinitions>()?.GetScene(TagName ?? string.Empty) ??
                               dom?.source?.GetElement<SceneDefinitions>()?.GetElement<Scene>();
                    if (target == null) return;

                    imgRef = target.GetParameter(ImageParameterString);
                    imageTarget = target.imageTarget;
                });
                return;
            }

            //Otherwise, check for an image reference
            imgRef ??= GetParameter(ImageParameterString);

            if (imgRef != null && imgRef.isReferencing!)
            {
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                ParserRef!.QueueForReferenceSolve(OnImgReferenceSolve);
                return;
            }
        }

        void OnImgReferenceSolve()
        {
            TagBase? source = null;
            TagBase? target = null;
            var image = GetParameterValue<string>(ImageParameterString);
            try
            {
                if (image == null || image?.ToLower() == NullParameterString)
                {
                    XVNMLLogger.LogWarning($"Image Source was set to null for: {TagName}", this);
                    return;
                }

                //Iterate through until you find the right source target;
                source = ParserRef!.root?.elements?
                    .Where(tag => tag.GetType() == typeof(ImageDefinitions))
                    .First();

                target = source?.GetElement<Image>(image!);
                imageTarget = (Image)Convert.ChangeType(target, typeof(Image))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {GetParameter(ImageParameterString)}" +
                    $"img {source!.tagTypeName}");
            }
        }
    }
}
