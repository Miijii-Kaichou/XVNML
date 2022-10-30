using XVNML.Core.Dialogue;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogue", TagOccurance.Multiple)]
    public class Dialogue : TagBase
    {
        public string? Script { get; private set; }
        public string? ID { get; private set; }
        public string? Name { get; private set; }
        public bool? DoNotDetain { get; private set; } = false;
        public Cast[]? includedCasts;

        public DialogueScript? dialogueOutput;

        public override void OnResolve()
        {
            base.OnResolve();
            Script = value?.ToString();
            ID = parameterInfo?["id"]?.ToString();
            Name = tagName;
            DoNotDetain = parameterInfo?["DoNotDetain"] != null;

            AnalyzeDialogue();
        }

        private void AnalyzeDialogue()
        {
            if (Script == null) throw new ArgumentNullException(nameof(Script));
            _ = new DialogueParser(Script, out dialogueOutput);
        }
    }
}
