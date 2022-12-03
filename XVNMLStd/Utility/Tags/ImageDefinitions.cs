using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("imageDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class ImageDefinitions : TagBase
    {
        public Image[]? Images => Collect<Image>();
        public Image? this[string name]
        {
            get { return GetImage(name); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Image? GetImage(string name) => this[name];

    }
}