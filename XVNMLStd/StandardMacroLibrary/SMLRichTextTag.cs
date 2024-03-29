﻿#pragma warning disable IDE0051 // Remove unused private members

using XVNML.Utilities.Macros;

namespace XVNML.StandardMacroLibrary
{
    /// <summary>
    /// *Disclaimer: Rich Text Tag Macros does not work on
    /// console applications.
    /// </summary>
    [MacroLibrary(typeof(SMLRichTextTag))]
    internal static class SMLRichTextTag
    {
        [Macro("aln")]
        [Macro("align")]
        private static void AlignMacro(MacroCallInfo info, string align)
        {
            info.process.AppendText($"<align=\"{align}\"/>");
        }

        [Macro("bed")]
        [Macro("bold_end")]
        private static void BoldEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</b>");
        }
        [Macro("b")]
        [Macro("bold")]
        private static void BoldMacro(MacroCallInfo info)
        {
            info.process.AppendText("<b>");
        }

        [Macro("coled")]
        [Macro("color_end")]
        private static void ColorEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</color>");
        }

        [Macro("col")]
        [Macro("color")]
        private static void ColorMacro(MacroCallInfo info, string value)
        {
            info.process.AppendText($"<color=\"{value}\">");
        }

        [Macro("emed")]
        [Macro("emphasis_end")]
        private static void EmphasisEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</i>");
        }

        [Macro("em")]
        [Macro("emphasis")]
        private static void EmphasisMacro(MacroCallInfo info)
        {
            info.process.AppendText("<i>");
        }

        [Macro("fed")]
        [Macro("font_end")]
        private static void FontEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</font>");
        }

        [Macro("f")]
        [Macro("font")]
        private static void FontMacro(MacroCallInfo info, string value)
        {
            info.process.AppendText($"<font=\"{value}\">");
        }

        [Macro("szed")]
        [Macro("size_end")]
        private static void FontSizeEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</size>");
        }

        [Macro("sz")]
        [Macro("size")]
        private static void FontSizeMacro(MacroCallInfo info, string value)
        {
            info.process.AppendText($"<size={value}%>");
        }

        [Macro("spr_end")]
        [Macro("sprite_end")]
        private static void SpriteEndMacro(MacroCallInfo info, string value)
        {
            info.process.AppendText($"<sprite name=\"{value}\">");
        }

        [Macro("spr")]
        [Macro("sprite")]
        private static void SpriteMacro(MacroCallInfo info, string value)
        {
            info.process.AppendText($"<sprite name=\"{value}\">");
        }

        [Macro("sed")]
        [Macro("strikethrough_end")]
        private static void StrikeThroughEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</strikethrough>");
        }

        [Macro("s")]
        [Macro("strikethrough")]
        private static void StrikeThroughMacro(MacroCallInfo info)
        {
            info.process.AppendText("<strikethrough>");
        }

        [Macro("stled")]
        [Macro("style_end")]
        private static void StyleEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</style>");
        }

        [Macro("stl")]
        [Macro("style")]
        private static void StyleMacro(MacroCallInfo info, string value)
        {
            info.process.AppendText($"<style=\"{value}\">");
        }

        [Macro("ued")]
        [Macro("underline_end")]
        private static void UnderlineEndMacro(MacroCallInfo info)
        {
            info.process.AppendText("</u>");
        }

        [Macro("u")]
        [Macro("underline")]
        private static void UnderlineMacro(MacroCallInfo info)
        {
            info.process.AppendText("<u>");

        }
    }
}
#pragma warning restore IDE0051 // Remove unused private members