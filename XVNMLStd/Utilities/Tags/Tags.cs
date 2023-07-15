using Newtonsoft.Json;
using System;
using System.Linq;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("tags", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Tags : TagBase
    {
        [JsonProperty] public string[]? list;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[] { ListParameterString };
            AllowedFlags = new[] { UseChildrenFlagString };

            base.OnResolve(fileOrigin);

            var stringValue = GetParameterValue<string>(ListParameterString);
            var useChilren = HasFlag(UseChildrenFlagString);

            if (stringValue != null || useChilren == false)
            {
                list = stringValue?.Split(ListDelimiters, StringSplitOptions.RemoveEmptyEntries);
                return;
            }

            if (useChilren == true) list = Collect<Tag>().Select(t => t.name).ToArray()!;
        }

        public bool IncludesTag(string tagName) => list!.Contains(tagName);
    }
}