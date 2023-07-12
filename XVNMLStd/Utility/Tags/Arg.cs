using Newtonsoft.Json;
using System;
using XVNML.Core.Extensions;
using XVNML.Core.Tags;
using XVNML.XVNMLUtility.Tags;

using static XVNML.Constants;

namespace XVNML.Utility.Tags
{
    [AssociateWithTag("arg", typeof(Macro), TagOccurance.Multiple)]
    public sealed class Arg : TagBase
    {
        [JsonProperty]
        public (object value, Type type) argData;


        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ValueParameterString
            };

            base.OnResolve(fileOrigin);

            argData.value = GetParameter(ValueParameterString)!;
            argData.type = argData.value.DetermineValueType()!;
        }
    }
}
