using System;
using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("tags", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Tags : TagBase
    {
        public string[]? list;
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            list = parameterInfo?["list"]?.ToString()?.Split(new[] { ',', ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
        public bool IncludesTag(string tagName) => list!.Contains(tagName);
    }
}