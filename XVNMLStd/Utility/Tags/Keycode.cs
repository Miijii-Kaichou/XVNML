using System;
using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("keycode", typeof(KeycodeDefinitions), TagOccurance.Multiple)]
    public sealed class Keycode : TagBase
    {
        public VirtualKey key;
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            key = (VirtualKey)Enum.Parse(typeof(VirtualKey), (string?)parameterInfo?["key"] ?? "Null");
        }
    }
}
