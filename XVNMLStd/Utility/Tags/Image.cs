using System;
using System.IO;
using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("image", typeof(ImageDefinitions), TagOccurance.Multiple)]
    public sealed class Image : TagBase
    {
        internal DirectoryRelativity relativity;
        internal DirectoryInfo? dirInfo;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            var rel = parameterInfo?["rel"];
            string src = (string?)parameterInfo?["src"] ?? string.Empty;
            relativity = rel == null ? default : (DirectoryRelativity)Enum.Parse(typeof(DirectoryRelativity), rel.ToString()!);
            var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + src : src;
            if (pathFlow == string.Empty) return;
            dirInfo = new DirectoryInfo(pathFlow);
        }
    }
}