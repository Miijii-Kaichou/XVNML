using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("portraitDefinitions", typeof(Cast), TagOccurance.PragmaLocalOnce)]
    sealed class PortraitDefinitions : TagBase
    {
        public Portrait[]? Portraits => ((IList<Portrait>)elements!).ToArray();
        public Portrait? this[string name]
        {
            get { return GetPortrait(name.ToString()); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Portrait? GetPortrait(string name) => this[name];
    }
}
