namespace XVNML.Core.Parser
{
    internal class DialogueParser
    {
        public DialogueParser(string source, out DialogueSetOuput output)
        {
            Source = source;
            Console.WriteLine(Source);
            output = ParseDialogue();
        }

        private DialogueSetOuput ParseDialogue()
        {
            return null;
        }

        public string Source { get; }
    }
}
