using System.Collections.Generic;

namespace XVNML.Core.Dialogue
{
    /// <summary>
    /// A class that takes a dialogue, and actually
    /// puts it into good use.
    /// </summary>
    public class DialogueScript
    {
        internal DialogueLine[] Lines => _lineList.ToArray();
        private readonly List<DialogueLine> _lineList = new List<DialogueLine>();

        public DialogueLine GetLine(int index) => Lines?[index]!;
        public void ComposeNewLine(DialogueLine? line)
        {
            if (line == null) return;
            _lineList.Add(line);
        }
    }
}
