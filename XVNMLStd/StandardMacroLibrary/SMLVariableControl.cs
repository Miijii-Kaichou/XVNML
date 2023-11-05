#pragma warning disable IDE0051 // Remove unused private members

using System;
using System.Collections.Generic;
using XVNML.Core.Extensions;
using XVNML.Core.Native;
using XVNML.Utilities.Dialogue;
using XVNML.Utilities.Macros;

namespace XVNML.StandardMacroLibrary
{
    [MacroLibrary(typeof(SMLVariableControl))]
    internal static class SMLVariableControl
    {
        [Macro("declare")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static void DeclareVariableMacro(MacroCallInfo info, string identifier, object _typeof)
        {
            var variableType = Type.GetType(_typeof.ToString());  
            
            if (variableType == null)
            {
                switch(_typeof.ToString())
                {
                    case "string": variableType = typeof(string); break;
                    case "int": variableType = typeof(int); break;
                    case "float": variableType = typeof(float); break;
                    case "double": variableType = typeof(double); break;
                    case "uint": variableType = typeof(uint); break;
                }
            }

            RuntimeReferenceTable.Declare(identifier.ToString(), variableType!);
        }

        [Macro("initialize")]
        private static void InitializeVariableMacro(MacroCallInfo info, string identifier, object _typeof, object initialValue)
        {
            DeclareVariableMacro(info, identifier, _typeof);
            SetVariableMacro(info, identifier.ToString(), initialValue);
        }

        [Macro("set")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static void SetVariableMacro(MacroCallInfo info, object identifier, object newValue)
        {
            RuntimeReferenceTable.Set(identifier.ToString(), newValue);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members