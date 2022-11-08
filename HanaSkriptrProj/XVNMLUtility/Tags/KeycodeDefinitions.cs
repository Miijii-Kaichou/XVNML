using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("keycodeDefinitions", typeof(Proxy), TagOccurance.PragmaOnce)]
    sealed class KeycodeDefinitions : TagBase
    {
        public Keycode[]? KeyCodes => Collect<Keycode>();
        public Keycode? this[string name]
        {
            get { return GetKeyCode(name); }
        }

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        public Keycode? GetKeyCode(string name) => this[name];
    }
}