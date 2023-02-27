using System;
using System.Runtime.Serialization;
using XVNML.Core.Dialogue;

namespace XVNML.Core.Macros
{
    [Serializable]
    internal class InvalidMacroException : Exception
    {
        public InvalidMacroException(string symbolName, DialogueLine source)
        {
            Console.Error.WriteLine($"Invalid macro by the name \"{symbolName}\" at line: \"{source.Content}\"");
        }
    }
}