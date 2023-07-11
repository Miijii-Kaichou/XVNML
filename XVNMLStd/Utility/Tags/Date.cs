using Newtonsoft.Json;
using System;
using System.Globalization;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
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
