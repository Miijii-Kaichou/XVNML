using Newtonsoft.Json;
using XVNML.Core.Dialogue.Enums;

namespace XVNML.Core.Dialogue.Structs
{
    internal struct LineDataInfo
    {
        [JsonProperty] internal int lineIndex;
        [JsonProperty] internal int returnPoint;
        [JsonProperty] internal bool isPartOfResponse;
        [JsonProperty] internal string? fromResponse;
        [JsonProperty] internal SkripterLine? parentLine;
        [JsonProperty] internal bool isClosingLine;
        [JsonProperty] internal string? responseString;

        public DialogueLineMode Mode { get; set; }
    }
}