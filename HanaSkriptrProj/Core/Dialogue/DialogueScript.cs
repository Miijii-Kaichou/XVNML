namespace XVNML.Core.Dialogue
{
    /// <summary>
    /// A class that takes a dialogue, and actually
    /// puts it into good use.
    /// </summary>
    public class DialogueScript
    {
        private DialogueLine[] Lines => lineList.ToArray();
        private List<DialogueLine>  lineList = new List<DialogueLine>();

        public DialogueLine GetLine(int index) => Lines?[index]!;
        public void ComposeNewLine(DialogueLine? line)
        {
            if (line == null) return;
            lineList.Add(line);
        }
    }
}
