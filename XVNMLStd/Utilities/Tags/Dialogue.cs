using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using XVNML.Core.Dialogue;
using XVNML.Core.Lexer;
using XVNML.Core.Parser;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;

using static XVNML.ParameterConstants;
using static XVNML.FlagConstants;
using static XVNML.DirectoryConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(Source), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    [Serializable()]
    public sealed class Dialogue : TagBase
    {
        const string _DialogueDir = DefaultDialogueDirectory;

        [JsonProperty] public SyntaxToken?[]? Script { get; private set; }
        [JsonProperty] public string? Name { get; private set; }
        [JsonProperty] public bool DoNotDetain { get; private set; } = false;
        [JsonProperty] public bool TextSpeedControlledExternally { get; private set; } = false;
        [JsonProperty] public bool EnableMigration { get; private set; } = false;
        [JsonProperty] public bool EnableGroupMigration { get; private set; } = false;

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
                TextSpeedControlledExternallyFlagString,
                EnableMigrationFlagString,
                EnableGroupMigrationFlagString
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
                    var target = dom?.source?.SearchElement<Dialogue>(TagName ?? string.Empty);
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
            if (value == null) return;
            Script = (SyntaxToken[])value!;
            Name = TagName;

            // Flags
            DoNotDetain = HasFlag(DontDetainFlagString);
            TextSpeedControlledExternally = HasFlag(TextSpeedControlledExternallyFlagString);

            AnalyzeDialogue();
        }

        private void AnalyzeDialogue()
        {
            if (Script == null) return;
            parserOrigin = new SkriptrParser(Script, out dialogueOutput);
        }
    }
}
