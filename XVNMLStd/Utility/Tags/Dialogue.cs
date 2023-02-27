using XVNML.Core.Dialogue;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(Source), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    public sealed class Dialogue : TagBase
    {
        public string? Script { get; private set; }
        public string? ID { get; private set; }
        public string? Name { get; private set; }
        public bool DoNotDetain { get; private set; } = false;
        public Cast[]? includedCasts;

        public DialogueScript? dialogueOutput;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            Script = value?.ToString();
            ID = parameterInfo?["id"]?.ToString();
            Name = tagName;

            // Flags
            DoNotDetain = parameterInfo!.HasFlag("dontDetain");

            AnalyzeDialogue();
        }

        private void AnalyzeDialogue()
        {
            if (Script == null) return;
            _ = new DialogueParser(Script, out dialogueOutput);
        }
    }
}
