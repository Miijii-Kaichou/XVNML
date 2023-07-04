using Newtonsoft.Json;
using System.Collections.Generic;

namespace XVNML.Core.Dialogue
{
    /// <summary>
    /// A class that takes a dialogue, and actually
    /// puts it into good use.
    /// </summary>
    public sealed class DialogueScript
    {
        [JsonProperty] public List<SkripterLine>? Lines { get; set; } = new List<SkripterLine>();

        public SkripterLine GetLine(int index) => Lines?[index]!;
        public void ComposeNewLine(SkripterLine? line)
        {
            if (line == null) return;
            Lines.Add(line);
        }
    }
}
