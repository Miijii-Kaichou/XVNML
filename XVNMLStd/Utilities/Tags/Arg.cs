using Newtonsoft.Json;
using System;
using XVNML.Core.Extensions;
using XVNML.Core.Tags;
using XVNML.Utilities.Tags;

using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("arg", typeof(Macro), TagOccurance.Multiple)]
    public sealed class Arg : TagBase
    {
        [JsonProperty]
        private (object value, Type type) _argData;
        public (object Value, Type Type) ArgData => _argData;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ValueParameterString
            };

            base.OnResolve(fileOrigin);

            _argData.value = GetParameterValue<object>(ValueParameterString)!;
            _argData.type = _argData.value.DetermineValueType()!;
        }
    }
}
