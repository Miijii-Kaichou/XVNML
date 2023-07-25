using System;
using XVNML.Core.Dialogue;
using XVNML.Utilities.Diagnostics;
using XVNML.Utilities;

namespace XVNML.Core.Macros
{
    [Serializable]
    public class InvalidMacroException : Exception
    {
        public InvalidMacroException(string message, string symbolName, SkripterLine source) : base(message)
        {
            XVNMLLogger.LogError(message, source, symbolName);
        }
    }
}