using XVNML.Core.Enum;
using XVNML.Core.Tags;

using static XVNML.Constants;

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
                EngineParameterString,
                TargetParameterString,
                InteroperableLanguageParameterString,
                ScreenWidthParameterString,
                ScreenHeightParameterString,
                AspectRatioParameterString
            };

            base.OnResolve(fileOrigin);
            engine = GetParameterValue<string>(EngineParameterString);
            target = GetParameterValue<string>(TargetParameterString);
            lang = GetParameterValue<TargetLanguage>(InteroperableLanguageParameterString);
            screenWidth = GetParameterValue<uint>(ScreenWidthParameterString);
            screenHeight = GetParameterValue<uint>(ScreenHeightParameterString);
            aspectRatio = GetParameterValue<string>(AspectRatioParameterString);
        }
    }
}
