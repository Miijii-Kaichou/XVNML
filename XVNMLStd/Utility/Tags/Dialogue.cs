using XVNML.Core.Dialogue;
using XVNML.Core.Parser;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;
using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(Source), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    public sealed class Dialogue : TagBase
    {
        const string _DialogueDir = DefaultDialogueDirectory;

        public string? Script { get; private set; }
        public string? Name { get; private set; }
        public bool DoNotDetain { get; private set; } = false;

        public Cast[]? includedCasts;
        public DialogueScript? dialogueOutput;

        internal SkriptrParser? parserOrigin;

        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                PathRelativityParameterString,
                SourceParameterString
            };

            AllowedFlags = new[]
            {
                DontDetainFlagString,
                AllowOverrideFlagString
            };

            base.OnResolve(fileOrigin);

            var source = GetParameterValue<string>(SourceParameterString);

            if (source?.ToLower() == NullParameterString)
            {
                XVNMLLogger.LogWarning($"Dialogue Source was set to null for: {TagName}", this);
                return;
            }

            if (source != null)
            {
                XVNMLObj.Create(fileOrigin + _DialogueDir + source!.ToString(), dom =>
                {
                    if (dom == null) return;
                    var target = dom?.source?.GetElement<Dialogue>(TagName ?? string.Empty) ??
                        dom?.source?.GetElement<Dialogue>();
                    if (target == null) return;
                    value = target.value;
                    TagName = target.TagName;
                    Configure();
                });
                return;
            }

            Configure();
        }

        private void Configure()
        {
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
