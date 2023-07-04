using System;
using XVNML.Core.Dialogue;
using XVNML.Utility.Diagnostics;

namespace XVNML.Core.Macros
{
    [Serializable]
    public class MacroNotImplementedException : Exception
    {
        public MacroNotImplementedException(string symbolName)
        {
            XVNMLLogger.LogError($"Macro \"{symbolName}\" has not been implemented.", this, this);
        }
    }
}