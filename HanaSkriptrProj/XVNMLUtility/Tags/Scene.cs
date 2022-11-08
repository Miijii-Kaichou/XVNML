using XVNML.Core.Parser;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("scene", typeof(SceneDefinitions), TagOccurance.Multiple)]
    sealed class Scene : TagBase
    {
        internal Image? img;
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            var imgRef = parameterInfo?.GetParameter("img");
            if (imgRef != null && imgRef.isReferencing!)
            {
                // We'll request a ReferenceSolve by stating who
                // we are, the value we want to resolve, the type of that
                // value we want to resolve, and where you may be able to resolve it.
                Parser.QueueForReferenceSolve(OnImgReferenceSolve);
                return;
            }
        }

        void OnImgReferenceSolve()
        {
            TagBase? source = null;
            TagBase? target = null;
            try
            {
                //Iterate through until you find the right source target;
                source = Parser.RootTag?.elements?.Where(tag => tag.GetType() == typeof(ImageDefinitions)).First();
                target = source?.GetElement<Image>(parameterInfo?.GetParameter("img").value?.ToString()!);
                img = (Image)Convert.ChangeType(target, typeof(Image))!;
            }
            catch
            {
                throw new Exception($"Could not find reference called {parameterInfo?.GetParameter("img").value?.ToString()!}" +
                    $"img {source.tagTypeName}");
            }
        }
    }
}
