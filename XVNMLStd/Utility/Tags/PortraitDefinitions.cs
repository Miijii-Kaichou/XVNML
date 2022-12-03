using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("portraitDefinitions", typeof(Cast), TagOccurance.PragmaLocalOnce)]
    public sealed class PortraitDefinitions : TagBase
    {
        public Portrait[]? Portraits => Collect<Portrait>();
        public Portrait? this[string name]
        {
            get { return GetPortrait(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Portrait? GetPortrait(string name) => this[name];
    }
}
