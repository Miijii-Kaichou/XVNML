using Newtonsoft.Json;
using System;
using XVNML.Core.Tags;
using XVNML.Input.Enums;
using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("keycode", new[] { typeof(Source), typeof(KeycodeDefinitions) }, TagOccurance.Multiple)]
    public sealed class Keycode : TagBase
    {
        [JsonProperty] public VirtualKey vkey;
        [JsonProperty] public InputEvent purpose;
        
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