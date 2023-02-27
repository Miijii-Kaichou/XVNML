using System;
using System.IO;
using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("audio", typeof(AudioDefinitions), TagOccurance.Multiple)]
    public sealed class Audio : TagBase
    {
        internal DirectoryRelativity relativity;
        internal DirectoryInfo? dirInfo;

        internal static Audio? First(Func<object, bool> value)
        {
            throw new NotImplementedException();
        }

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                "rel",
                "src"
            };

            base.OnResolve(fileOrigin);
            
            var rel = GetParameterValue("rel");
            string src = (string?)GetParameterValue("src") ?? string.Empty;
            relativity = rel == null ? default : (DirectoryRelativity)Enum.Parse(typeof(DirectoryRelativity), rel.ToString()!);
            var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + src : src;
            dirInfo = new DirectoryInfo(pathFlow);
        }
    }
}