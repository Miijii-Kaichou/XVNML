using XVNML.Core.Dialogue;
using XVNML.Core.Tags;
using XVNML.Core.Parser;
using XVNML.Utility.Diagnostics;
using System;
using System.Linq;

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
                "dontDetain",
                "allowOverride"
            };

            base.OnResolve(fileOrigin);
            
            Script = value?.ToString();
            Name = TagName;

            // Flags
            DoNotDetain = HasFlag("dontDetain");

            AnalyzeDialogue();
        }

        private void AnalyzeDialogue()
        {
            if (Script == null) return;
            parserOrigin = new SkriptrParser(Script, out dialogueOutput);
        }
    }
}
