using XVNML.Core.Dialogue;
using XVNML.Core.Tags;
using XVNML.Core.Parser;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(Source), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    public sealed class Dialogue : TagBase
    {
        public string? Script { get; private set; }
        public string? Name { get; private set; }
        public bool DoNotDetain { get; private set; } = false;

        public Cast[]? includedCasts;
        public DialogueScript? dialogueOutput;

        internal SkriptrParser? parserOrigin;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedFlags = new[]
            {
                DontDetainFlagString,
                AllowOverrideFlagString
            };

            base.OnResolve(fileOrigin);

            var source = GetParameterValue<string>(SourceParameterString);

            Script = value?.ToString();
            Name = TagName;

            // Flags
            DoNotDetain = HasFlag(DontDetainFlagString);

            AnalyzeDialogue();
        }

        private void AnalyzeDialogue()
        {
            if (Script == null) return;
            parserOrigin = new SkriptrParser(Script, out dialogueOutput);
        }
    }
}
