using System;
using System.Linq;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("scene", typeof(SceneDefinitions), TagOccurance.Multiple)]
    public sealed class Scene : TagBase
    {
        public string _SceneDir = @"\Scenes\";
        public Image? imageTarget;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                "src",
                "img"
            };

            base.OnResolve(fileOrigin);

            // Will evaluation a source first if one exists
            var source = GetParameterValue(AllowedParameters[0]);

            TagParameter? imgRef = null;

            if (source != null)
            {
                if (source?.ToString().ToLower() == "nil")
                {
                    XVNMLLogger.LogWarning($"Scene Source was set to null for: {TagName}", this);
                    return;
                }

                XVNMLObj.Create(fileOrigin + _SceneDir + source!.ToString(), dom =>
                {
                    if (dom == null) return;

                    var target = dom?.source?.GetElement<SceneDefinitions>()?.GetScene(TagName ?? string.Empty) ??
                               dom?.source?.GetElement<SceneDefinitions>()?.GetElement<Scene>();
                    if (target == null) return;

                    imgRef = target.GetParameter("img");
                    imageTarget = target.imageTarget;
                });
                return;
            }

            //Otherwise, check for an image reference
            imgRef ??= GetParameter("img");

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
            var image = GetParameterValue("img")?.ToString()!;
            try
            {
                if (image == null || image?.ToString().ToLower() == "nil")
                {
                    XVNMLLogger.LogWarning($"Image Source was set to null for: {TagName}", this);
                    return;
                }

                //Iterate through until you find the right source target;
                source = ParserRef!._rootTag?.elements?
                    .Where(tag => tag.GetType() == typeof(ImageDefinitions))
                    .First();

                target = source?.GetElement<Image>(image);
                imageTarget = (Image)Convert.ChangeType(target, typeof(Image))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {GetParameter("img")}" +
                    $"img {source!.tagTypeName}");
            }
        }
    }
}
