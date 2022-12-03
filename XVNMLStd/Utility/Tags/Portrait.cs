using System;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("portrait", typeof(PortraitDefinitions), TagOccurance.Multiple)]
    public sealed class Portrait : TagBase
    {
        internal Image? img;
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            var imgRef = parameterInfo?.GetParameter("img");
            if (imgRef != null && imgRef.isReferencing)
            {
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                parserRef!.QueueForReferenceSolve(OnImgReferenceSolve);
                return;
            }

            throw new Exception("\"img\" parameter must pass a reference");
        }

        void OnImgReferenceSolve()
        {
            TagBase? source = null;
            TagBase? target = null;
            try
            {
                //Iterate through until you find the right source target;
                source = parserRef!._rootTag?.elements?.Where(tag => tag.GetType() == typeof(ImageDefinitions)).First();
                target = source?.GetElement<Image>(parameterInfo?.GetParameter("img").value?.ToString()!);
                img = (Image)Convert.ChangeType(target, typeof(Image))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {parameterInfo?.GetParameter("img").value?.ToString()!}" +
                    $"img {source!.tagTypeName}");
            }
        }
    }
}