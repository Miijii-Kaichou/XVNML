using Newtonsoft.Json;
using System;
using System.Globalization;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;
using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("date", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Date : TagBase
    {
        [JsonProperty] public DateTime date;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[] {
                ValueParameterString
            };

            base.OnResolve(fileOrigin);

            var valueParameter = GetParameterValue<string>(ValueParameterString);
            if (valueParameter == null) return;

            date = DateTime.ParseExact(valueParameter, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        }
    }
}
