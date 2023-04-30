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
        internal byte[] data;

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
            var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + "Audio\\" + src : src;
            //var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + XVNMLConfig.RelativePath["Audio"] + src : src;
            if (pathFlow == string.Empty) return;
            dirInfo = new DirectoryInfo(pathFlow);
            ProcessData();
        }

        private void ProcessData()
        {
            if (dirInfo == null) return;
            if (File.Exists(GetAudioTargetPath()) == false) return;
            data = File.ReadAllBytes(GetAudioTargetPath());
        }

        public byte[] GetAudioData() { return data; }
        public string? GetAudioTargetPath() { return dirInfo?.FullName; }
    }
}