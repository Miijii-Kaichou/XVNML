using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("keycode", typeof(KeycodeDefinitions), TagOccurance.Multiple)]
    sealed class Keycode : TagBase
    {
        public VirtualKey key;
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            key = (VirtualKey)Enum.Parse(typeof(VirtualKey), (string?)parameterInfo?["key"] ?? "Null");
        }
    }
}
