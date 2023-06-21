using System;
using XVNML.Core.Dialogue;

namespace XVNML.Core.Macros
{
    [Serializable]
    internal class InvalidMacroException : Exception
    {
        public InvalidMacroException(string symbolName, SkripterLine source)
        {
            Console.Error.WriteLine($"Invalid macro by the name \"{symbolName}\" at line: \"{source.Content}\"");
        }
    }
}