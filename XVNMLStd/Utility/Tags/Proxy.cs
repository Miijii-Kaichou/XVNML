using System;
using XVNML.Core;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("proxy", TagOccurance.PragmaOnce)]
    public sealed class Proxy : TagBase
    {
        public string? engine;
        public string? target;
        public TargetLanguage lang;
        public uint? screenWidth;
        public uint? screenHeight;
        public string? aspectRatio;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            engine = parameterInfo?["engine"]!.ToString()!;
            target = parameterInfo?["target"]!.ToString()!;
            lang = Enum.Parse<TargetLanguage>(parameterInfo?.paramters["lang"].value?.ToString()!);
            screenWidth = Convert.ToUInt32(parameterInfo?["screenWidth"]);
            screenHeight = Convert.ToUInt32(parameterInfo?["screenHeight"]);
            aspectRatio = parameterInfo?["aspectRatio"]!.ToString();
        }
    }
}
