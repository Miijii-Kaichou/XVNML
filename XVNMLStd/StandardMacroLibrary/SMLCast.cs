#pragma warning disable IDE0051 // Remove unused private members

using XVNML.Utilities.Macros;

namespace XVNML.StandardMacroLibrary
{
    [MacroLibrary(typeof(SMLCast))]
    internal static class SMLCast
    {
        [Macro("expression")]
        [Macro("portrait")]
        [Macro("exp")]
        [Macro("port")]
        private static void SetCastExpressionMacro(MacroCallInfo info, string value)
        {
            info.process.ChangeCastExpression(info, value);
        }

        [Macro("expression")]
        [Macro("portrait")]
        [Macro("exp")]
        [Macro("port")]
        private static void SetCastExpressionMacro(MacroCallInfo info, int value)
        {
            SetCastExpressionMacro(info, value.ToString());
        }

        [Macro("voice")]
        [Macro("vo")]
        private static void SetCastVoice(MacroCallInfo info, string value)
        {
            info.process.ChangeCastVoice(info, value);
        }

        [Macro("voice")]
        [Macro("vo")]
        private static void SetCastVoice(MacroCallInfo info, int value)
        {
            SetCastVoice(info, value.ToString());
        }
    }
}

#pragma  warning restore IDE0051 // Remove unused private members