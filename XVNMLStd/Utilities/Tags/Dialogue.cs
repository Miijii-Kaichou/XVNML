using Newtonsoft.Json;
using System;
using System.Text;
using XVNML.Core.Dialogue;
using XVNML.Core.Lexer;
using XVNML.Core.Parser;
using XVNML.Core.Tags;
using XVNML.Utilities.Diagnostics;

using static XVNML.ParameterConstants;
using static XVNML.FlagConstants;
using static XVNML.DirectoryConstants;
using XVNML.Utilities.Object;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("dialogue", new[] { typeof(Proxy), typeof(Source), typeof(DialogueGroup) }, TagOccurance.Multiple)]
    [Serializable()]
    public sealed class Dialogue : TagBase
    {
        [JsonProperty] public SyntaxToken?[]? Script { get; private set; }
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
                DontDetainFlagString
            };

            base.OnResolve(fileOrigin);

            DirectoryRelativity rel = GetParameterValue<DirectoryRelativity>(PathRelativityParameterString);
            string source = GetParameterValue<string>(SourceParameterString);

            if (source?.ToLower() == NullParameterString)
            {
                XVNMLLogger.LogWarning($"Dialogue Source was set to null for: {TagName}", this);
                return;
            }

            if (source != null)
            {
                string sourcePath = rel == DirectoryRelativity.Absolute
                    ? source
                    : new StringBuilder()
                    .Append(fileOrigin)
                    .Append(DefaultDialogueDirectory)
                    .Append(source)
                    .ToString()!;

                XVNMLObj.Create(sourcePath, dom =>
                {
                    if (dom == null) return;

                    var target = dom?.source?.SearchElement<Dialogue>(TagName ?? string.Empty);
                    if (target == null) return;

                    value = target.value;
                    TagName = target.TagName;
                    RootScope = dom?.Root?.TagName;
                    ProcessData();
                });
                return;
            }

            ProcessData();
        }

        private void ProcessData()
        {
            if (value == null) return;

            Name = TagName;

            if ((value is SyntaxToken[]) == false) return;

            Script = (SyntaxToken[])value!;
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
