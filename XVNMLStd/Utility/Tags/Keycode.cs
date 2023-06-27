using System;
using XVNML.Core.Tags;
using XVNML.Input.Enums;
using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("keycode", typeof(KeycodeDefinitions), TagOccurance.Multiple)]
    public sealed class Keycode : TagBase
    {
        public VirtualKey vkey;
        public InputEvent purpose;
        
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                KeyParameterString,
                InputPurposeParameterString
            };

            base.OnResolve(fileOrigin);
            vkey = GetParameterValue<VirtualKey>(KeyParameterString);
            purpose = GetParameterValue<InputEvent>(InputPurposeParameterString);
        }
    }
}