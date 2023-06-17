using System;
using System.IO;
using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("image", typeof(ImageDefinitions), TagOccurance.Multiple)]
    public sealed class Image : TagBase
    {
        internal DirectoryRelativity relativity;
        internal DirectoryInfo? dirInfo;
        internal byte[] data;

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

            if (src?.ToString().ToLower() == "nil")
            {
                XVNMLLogger.LogWarning($"Image Source was set to null for: {TagName}", this);
                return;
            }

            relativity = rel == null ? default : (DirectoryRelativity)Enum.Parse(typeof(DirectoryRelativity), rel.ToString()!);
            var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + "Images\\" + src : src;
            //var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + XVNMLConfig.RelativePath["Image"] + src : src;
            if (pathFlow == string.Empty) return;
            dirInfo = new DirectoryInfo(pathFlow);
            ProcessData();
        }

        public override string ToString()
        {
            return dirInfo == null ? string.Empty : dirInfo!.FullName;
        }

        private void ProcessData()
        {
            if (dirInfo == null) return;
            if (File.Exists(GetImageTargetPath()) == false)
            {
                XVNMLLogger.LogError($"The path {dirInfo.FullName} doesn't exist.", this, this);
                return;
            }
            data = File.ReadAllBytes(GetImageTargetPath());
        }

        public byte[] GetImageData() { return data; }

        public string? GetImageTargetPath() { return dirInfo?.FullName; }
    }
}