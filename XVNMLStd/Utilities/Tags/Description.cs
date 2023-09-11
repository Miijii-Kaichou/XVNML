using Newtonsoft.Json;
using XVNML.Core.Tags;

using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("description", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Description : TagBase
    {
        [JsonProperty] public string? content;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ContentParameterString
            };

            base.OnResolve(fileOrigin);

            content = (value ?? GetParameterValue<string>(AllowedParameters[0])).ToString(); 
        }
    }
}
