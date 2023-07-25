using Newtonsoft.Json;
using XVNML.Core.Enums;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("proxy", TagOccurance.PragmaOnce)]
    public sealed class Proxy : TagBase
    {
        [JsonProperty] public string? engine;
        [JsonProperty] public string? target;
        [JsonProperty] public TargetLanguage lang;
        [JsonProperty] public uint? screenWidth;
        [JsonProperty] public uint? screenHeight;
        [JsonProperty] public string? aspectRatio;

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
