using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;
using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("copyright", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Copyright : TagBase
    {
        [JsonProperty] public string? fullCopyrightString;
        [JsonProperty] public int copyrightYear;
        [JsonProperty] public string? copyrightOwner;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                OwnerParameterString,
                YearParameterString
            };

            base.OnResolve(fileOrigin);

            char[] parenthesisDelimiters = new char[] { '(', ')' };
            char[] whitespaceDelimiters = new char[] {' ', '\r', '\n', '\t'};
            int i = 0;
            
            StringBuilder sb = new StringBuilder();

            copyrightOwner = GetParameterValue<string>(OwnerParameterString);
            copyrightYear = Convert.ToInt32(GetParameterValue<string>(YearParameterString));

            sb.Append("\u2122 ");
            sb.Append(parenthesisDelimiters[i++]);
            sb.Append(copyrightYear);
            sb.Append(parenthesisDelimiters[i++]); 
            
            i = 0;
            
            sb.Append(whitespaceDelimiters[i]);
            sb.Append(copyrightOwner);

            fullCopyrightString = sb.ToString();
        }
    }
}
