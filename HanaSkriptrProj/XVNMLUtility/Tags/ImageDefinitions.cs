using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("imageDefinitions", typeof(Proxy), TagOccurance.PragmaOnce)]
    sealed class ImageDefinitions : TagBase
    {
        public Image[]? Images => Collect<Image>();
        public Image? this[string name]
        {
            get { return GetImage(name); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Image? GetImage(string name) => this[name];

    }
}