using System;
using XVNML.Core.Enum;
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
            AllowedParameters = new[]
            {
                "engine",
                "target",
                "lang",
                "screenWidth",
                "screenHeight",
                "aspectRation"
            };

            base.OnResolve(fileOrigin);
            engine = GetParameterValue("engine")?.ToString()!;
            target = GetParameterValue("target")?.ToString()!;
            lang = Enum.Parse<TargetLanguage>(GetParameterValue("lang")?.ToString()!);
            screenWidth = Convert.ToUInt32(GetParameterValue("screenWidth"));
            screenHeight = Convert.ToUInt32(GetParameterValue("screenHeight"));
            aspectRatio = GetParameterValue("aspectRatio")?.ToString();
        }
    }
}
