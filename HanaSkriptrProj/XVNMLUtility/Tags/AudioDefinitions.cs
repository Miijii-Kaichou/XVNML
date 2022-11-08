using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("audioDefinitions", typeof(Proxy), TagOccurance.PragmaOnce)]
    sealed class AudioDefinitions : TagBase
    {
        public Audio[]? AudioCollection => Collect<Audio>();
        public Audio? this[string name]
        {
            get { return GetCast(name.ToString()); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Audio? GetCast(string name) => this[name];
    }
}
