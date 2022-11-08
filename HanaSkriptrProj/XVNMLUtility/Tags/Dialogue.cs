using XVNML.Core.Dialogue;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    sealed class Dialogue : TagBase
    {
        public string? Script { get; private set; }
        public string? ID { get; private set; }
        public string? Name { get; private set; }
        public bool? DoNotDetain { get; private set; } = false;
        public Cast[]? includedCasts;

        public DialogueScript? dialogueOutput;

        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            Script = value?.ToString();
            ID = parameterInfo?["id"]?.ToString();
            Name = tagName;
            DoNotDetain = parameterInfo?["DoNotDetain"] != null;

            AnalyzeDialogue();
        }

        private void AnalyzeDialogue()
        {
            if (Script == null) return;
            _ = new DialogueParser(Script, out dialogueOutput);
        }
    }
}
