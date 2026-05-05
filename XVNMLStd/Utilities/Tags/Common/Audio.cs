using Newtonsoft.Json;
using System;
using System.IO;
using XVNML.Core.Tags;
using XVNML.Core.Tags.Attributes;
using static XVNML.ParameterConstants;

namespace XVNML.Utilities.Tags.Common
{
    [AssociateWithTag("audio", typeof(AudioDefinitions), TagOccurance.Multiple)]
    public sealed class Audio : TagBase
    {
        [JsonProperty] internal DirectoryRelativity relativity;
        [JsonProperty] internal string? audioPath;
        [JsonProperty] internal byte[] data;

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
            relativity = rel;
            var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + "Audio\\" + src : src;
            //var pathFlow = relativity == DirectoryRelativity.Relative ? fileOrigin + @"\" + XVNMLConfig.RelativePath["Audio"] + src : src;
            if (pathFlow == string.Empty) return;
            audioPath = new DirectoryInfo(pathFlow).FullName;
            ProcessData();
        }

        private void ProcessData()
        {
            if (audioPath == null) return;
            if (File.Exists(GetAudioTargetPath()) == false) return;
            data = File.ReadAllBytes(GetAudioTargetPath());
        }

        public byte[] GetAudioData() { return data; }
        public string? GetAudioTargetPath() { return audioPath; }
    }
}