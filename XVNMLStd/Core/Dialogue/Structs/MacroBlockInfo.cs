using Newtonsoft.Json;
using System;

namespace XVNML.Core.Dialogue.Structs
{
    internal struct MacroBlockInfo
    {
        [JsonProperty] internal int blockPosition;
        [JsonProperty] internal (string macroSymbol, (object, Type)[] args, bool isReference, string? parent)[] macroCalls;

        public bool IsDefinedInDom { get; internal set; }

        internal void Initialize(int size)
        {
            macroCalls = new (string macroSymbol, (object, Type)[] args, bool isReference, string? parent)[size];
        }
        internal void SetPosition(int position)
        {
            blockPosition = position;
        }
    }
}