using System;
using System.IO;
using XVNML.Core.IO.Enums;
using XVNML.Core.Tags;

using static XVNML.Constants;

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
                PathRelativityParameterString,
                SourceParameterString
            };

            base.OnResolve(fileOrigin);
            
            var rel = GetParameterValue<DirectoryRelativity>(PathRelativityParameterString);
            string src = GetParameterValue<string>(SourceParameterString);
            relativity = rel == null ? default : rel;
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