﻿using System;
using XVNML.Core.Dialogue;
using XVNML.Utility.Diagnostics;

namespace XVNML.Core.Macros
{
    [Serializable]
    internal class InvalidMacroException : Exception
    {
        public InvalidMacroException(string message, string symbolName, SkripterLine source) : base(message)
        {
            XVNMLLogger.LogError(message, source, symbolName);
        }
    }
}