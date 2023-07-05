using Newtonsoft.Json;
using System;
using XVNML.Core.Dialogue;
using XVNML.Core.Parser;
using XVNML.Core.Tags;
using XVNML.Utility.Diagnostics;
using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(Source), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    [Serializable()]
    public sealed class Dialogue : TagBase
    {
        const string _DialogueDir = DefaultDialogueDirectory;

        [JsonProperty] public string? Script { get; private set; }
        [JsonProperty] public string? Name { get; private set; }
        [JsonProperty] public bool DoNotDetain { get; private set; } = false;

        [JsonProperty] public Cast[]? includedCasts;
        [JsonProperty] public DialogueScript? dialogueOutput;

        internal SkriptrParser? parserOrigin;

        public override void OnResolve(string? fileOrigin)
        {

            AllowedParameters = new[]
            {
                PathRelativityParameterString,
                SourceParameterString,
            };

            AllowedFlags = new[]
            {
                DontDetainFlagString,
                AllowOverrideFlagString,
                TextSpeedControlledExternallyFlagString
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
