﻿#pragma warning disable IDE0051 // Remove unused private members

using XVNML.Core.Macros;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities.Macros;
using XVNML.Utilities.Tags;

namespace XVNML.StandardMacroLibrary
{
    [MacroLibrary(typeof(SMLDebug))]
    internal static class SMLDebug
    {
        [Macro("curdex")]
        private static void CursorIndexMacro(MacroCallInfo info, bool print)
        {
            var cursorIndex = info.process.cursorIndex;
            XVNMLLogger.Log(cursorIndex.ToString(), info);
            if (!print) return;
            info.process.AppendText(cursorIndex.ToString());
        }

        [Macro("lindex")]
        private static void GetLineIndexMacro(MacroCallInfo info)
        {
            var print = false;
            GetLineIndexMacro(info, print);
        }

        [Macro("lindex")]
        private static void GetLineIndexMacro(MacroCallInfo info, bool print)
        {
            var lineIndex = info.process.lineIndex;
            XVNMLLogger.Log(lineIndex.ToString(), info);
            if (!print) return;
            info.process.AppendText(lineIndex.ToString());
        }

        [Macro("process_id")]
        private static void GetProcessIDMacro(MacroCallInfo info, bool print)
        {
            if (!print) return;
            info.process.AppendText(info.process.ID.ToString());
        }
        [Macro("pid")]
        private static void GetProcessIDMacroShortHand(MacroCallInfo info, bool print)
        {
            GetProcessIDMacro(info, print);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members