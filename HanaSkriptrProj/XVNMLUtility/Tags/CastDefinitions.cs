using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("castDefinitions", typeof(Proxy), TagOccurance.PragmaOnce)]
    sealed class CastDefinitions : TagBase
    {
        public Cast[]? CastMembers => Collect<Cast>();
        public Cast? this[string name]
        {
            get { return GetCast(name); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        public Cast? GetCast(string name) => this[name];
    }
}
