#pragma warning disable IDE0051 // Remove unused private members

using System;
using XVNML.Utilities.Macros;

namespace XVNML.StandardMacroLibrary
{
    [MacroLibrary(typeof(SMLVariableControl))]
    internal static class SMLVariableControl
    {
        [Macro("get")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static void GetVariableMacro(MacroCallInfo info, string identifier)
        {
            Console.WriteLine($"Getting Variable \"{identifier}\": <undefined>");
        }
        [Macro("declare")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static void InitializeVariableMacro(MacroCallInfo info, string identifier, object initialValue)
        {
            Console.WriteLine($"Variable {identifier} initiated with {initialValue}");
        }

        [Macro("set")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static void SetVariableMacro(MacroCallInfo info, string identifier, object newValue)
        {
            Console.WriteLine($"Setting Variable \"{identifier}\": {newValue}");
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members