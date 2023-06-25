using System;
using System.Linq;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("tags", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Tags : TagBase
    {
        public string[]? list;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[] { ListParameterString };
            base.OnResolve(fileOrigin);
            list = GetParameterValue<string>(ListParameterString)?
                .Split(ListDelimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        public bool IncludesTag(string tagName) => list!.Contains(tagName);
    }
}