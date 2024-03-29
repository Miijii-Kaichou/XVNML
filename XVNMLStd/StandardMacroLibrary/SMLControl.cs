﻿#pragma warning disable IDE0051 // Remove unused private members

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using XVNML.Core.Native;
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

            RuntimeReferenceTable.ProcessVariableExpression(text, _myVariable =>
            {
                info.process.AppendText(_myVariable?.ToString()!);
            }, () => info.process.AppendText(finalText.ToString()));
        }

        [Macro("insd")]
        [Macro("insert_directly")]
        private static void InsertDirectlyMacro(MacroCallInfo info, string value)
        {
            RuntimeReferenceTable.ProcessVariableExpression(value, _myVariable =>
            {
                info.process.AppendTextDirectly(_myVariable?.ToString()!);
            }, () => info.process.AppendTextDirectly(value));
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
            info.process.SetProcessRate(level == 0 ? level : 1000 / level);
        }

        [Macro("is_prime")]
        private static void IsPrimeMacro(MacroCallInfo info, uint value)
        {
            for (int i = 1; i < value; i++)
            {
                if (i != 1 && i < value && (value%i) == 0)
                {
                    info.process.AppendTextDirectly("false");
                    return;
                }
            }

            info.process.AppendTextDirectly("true");
        }

        [Macro("test")]
        private static void TestMacro(MacroCallInfo info, string name, int value)
        {
            info.process.AppendTextDirectly($"{name} has the value of [{value}]");
        }
    }
}
#pragma warning restore IDE0051 // Remove unused private members