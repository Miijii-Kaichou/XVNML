using System;
using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("keycode", typeof(KeycodeDefinitions), TagOccurance.Multiple)]
    public sealed class Keycode : TagBase
    {
        public VirtualKey key;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                KeyParameterString
            };

            base.OnResolve(fileOrigin);
            key = GetParameterValue<VirtualKey>(KeyParameterString);
        }
    }
}
