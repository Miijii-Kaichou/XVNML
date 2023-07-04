namespace XVNML
{
    internal static class Constants
    {
        internal const string NullParameterString = "nil";
        internal const string SourceParameterString = "src";
        internal const string ImageParameterString = "img";
        internal const string AudioParameterString = "audio";
        internal const string ListParameterString = "list";
        internal const string EngineParameterString = "engine";
        internal const string TargetParameterString = "target";
        internal const string InteroperableLanguageParameterString = "lang";
        internal const string ScreenWidthParameterString = "screenWidth";
        internal const string ScreenHeightParameterString = "screenHeight";
        internal const string AspectRatioParameterString = "aspectRatio";
        internal const string KeyParameterString = "vkey";
        internal const string InputPurposeParameterString = "purpose";
        internal const string PathRelativityParameterString = "pathMode";
        internal const string ValueParameterString = "value";
        internal const string OwnerParameterString = "owner";
        internal const string YearParameterString = "year";
        internal const string ContentParameterString = "content";


        internal const string ActAsSceneControllerFlagString = "actAsSceneController";
        internal const string AllowOverrideFlagString = "allowOverride";
        internal const string TextSpeedControlledExternallyFlagString = "textSpeedControlledExternally";
        internal const string DontDetainFlagString = "dontDetain";

        internal const string DefaultCastDirectory = @"\Casts\";
        internal const string DefaultDialogueDirectory = @"\Dialogue\";
        internal const string DefaultImageDirectory = @"\Images\";
        internal const string DefaultAudioDirectory = @"\Audio\";
        internal const string DefaultSceneDirectory = @"\Scenes\";

        internal static readonly char[] ListDelimiters =
        {
            ',',
            ' ',
            '\r',
            '\n'
        };
    }
}
