using System;
using System.Runtime.Serialization;

namespace XVNML.Core.Macros
{
    [Serializable]
    internal class InvalidMacroException : Exception
    {
        public InvalidMacroException(string symbolName)
        {
            Console.Error.WriteLine($"Invalid macro by the name {symbolName}");
        }
    }
}