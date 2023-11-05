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
        [JsonProperty] public List<SkriptrLine>? Lines { get; set; } = new List<SkriptrLine>();

        public SkriptrLine GetLine(int index) => Lines?[index]!;
        public void ComposeNewLine(SkriptrLine? line)
        {
            if (line == null) return;
            Lines?.Add(line);
        }
    }
}
