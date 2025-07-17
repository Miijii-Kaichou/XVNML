using Newtonsoft.Json;
using System.IO;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;

using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("image", new[] { typeof(Source), typeof(ImageDefinitions) }, TagOccurance.Multiple)]
    public sealed class Image : TagBase
    {
        [JsonProperty] private DirectoryRelativity relativity;
        [JsonProperty] internal string? imagePath;
        [JsonProperty] internal byte[]? data;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                PathRelativityParameterString,
                SourceParameterString
            };

            base.OnResolve(fileOrigin);

            DirectoryRelativity rel = GetParameterValue<DirectoryRelativity>(PathRelativityParameterString);
            string src = GetParameterValue<string>(SourceParameterString);

            if (src?.ToString().ToLower() == NullParameterString)
            {
                XVNMLLogger.LogWarning($"Image Source was set to null for: {TagName}", this);
                return;
            }

            relativity = rel;

            var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + "Images\\" + src : src;
            if (pathFlow == null) return;
            if (pathFlow == string.Empty) return;

            imagePath = new DirectoryInfo(pathFlow).FullName;
            ProcessData();
        }

        public override string ToString()
        {
            return imagePath == null ? string.Empty : imagePath!;
        }

        private void ProcessData()
        {
            if (imagePath == null) return;
            if (File.Exists(GetImageTargetPath()) == false)
            {
                XVNMLLogger.LogError($"The path {imagePath} doesn't exist.", this, this);
                return;
            }
            data = File.ReadAllBytes(GetImageTargetPath());
        }

        public byte[]? GetImageData() { return data; }

        public string? GetImageTargetPath() { return imagePath; }
    }
}