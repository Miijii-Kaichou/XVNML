﻿#pragma warning disable IDE0051 // Remove unused private members
using System;
using System.Linq;
using System.Text;
using XVNML.Utilities.Macros;

namespace XVNML.StandardMacroLibrary
{
    [MacroLibrary(typeof(SMLControl))]
    internal static class SMLControl
    {
        [Macro("clr")]
        [Macro("clear")]
        private static void ClearText(MacroCallInfo info)
        {
            info.process.Clear();
        }
        [Macro("del")]
        [Macro("delay")]
        private static void DelayMacro(MacroCallInfo info, uint milliseconds)
        {
            // Delay macro logic here.
            info.process.Wait(milliseconds);
        }

        [Macro("end")]
        private static void EndDialogueMacro(MacroCallInfo info)
        {
            info.process.lineIndex = info.process.lineProcesses.Count;
        }

        [Macro("ins")]
        [Macro("insert")]
        private static void InsertMacro(MacroCallInfo info, string text)
        {
            byte[] textBytes = Encoding.Unicode.GetBytes(text);
            var finalText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, textBytes));

            // Insert macro logic here.
            info.process.Append(finalText.ToString());
        }

        [Macro("jmpt")]
        [Macro("jump_to")]
        private static void JumpToMacro(MacroCallInfo info, uint index)
        {
            info.process.JumpTo((int)index);
        }

        [Macro("jmpt")]
        [Macro("jump_to")]
        private static void JumpToMacro(MacroCallInfo info, string tagName)
        {
            if (info.process.lineProcesses.Where(sl => sl.Name == tagName.ToString()).Any() == false) return;

            info.process.JumpTo(tagName.ToString());
        }

        [Macro("ldt")]
        [Macro("lead_to")]
        private static void LeadToLineMacro(MacroCallInfo info, int value)
        {
            info.process.LeadTo(value);
        }


        [Macro("op")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private static void OperationsVariableMacro(MacroCallInfo info, string expression)
        {
            Console.WriteLine($"Operate With Expression: \"{expression}\"");
        }

        [Macro("pass")]
        private static void PassMacro(MacroCallInfo info)
        {
            info.process.AllowPass();
        }

        [Macro("pause")]
        private static void PauseMacro(MacroCallInfo info)
        {
            info.process.Pause();
        }

        [Macro("sts")]
        [Macro("set_text_speed")]
        private static void SetTextSpeed(MacroCallInfo info, uint level)
        {
            // Speed macro logic here.
            info.process.SetProcessRate(level == 0 ? level : 1000 / level);
        }
    }
}
#pragma warning restore IDE0051 // Remove unused private members