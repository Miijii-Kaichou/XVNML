using Newtonsoft.Json;
using System;
using XVNML.Core.Tags;

using static XVNML.FlagConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("dialogueGroup", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.Multiple)]
    public sealed class DialogueGroup : TagBase
    {
        protected override string[]? AllowedFlags => new[]
        {
            ActAsSceneControllerFlagString,
            CollectFilesFlagString,
            EnableMigrationFlagString,
            EnableGroupMigrationFlagString
        };

        [JsonProperty] public bool IsActingAsSceneController { get; private set; }
        [JsonProperty] public bool CollectFiles { get; private set; }
        [JsonProperty] public bool EnableMigration { get; private set; }
        [JsonProperty] public bool EnableGroupMigration { get; private set; }

        public Dialogue? this[int index]
        {
            get { return GetDialogue(index); }
        }

        public new Dialogue? this[ReadOnlySpan<char> name]
        {
            get { return GetDialogue(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);

            // Flags
            int flagID = 0;

            IsActingAsSceneController = HasFlag(AllowedFlags![flagID++]);
            CollectFiles = HasFlag(AllowedFlags![flagID++]);
            EnableMigration = HasFlag(AllowedFlags![flagID++]);
            EnableGroupMigration = HasFlag(AllowedFlags![flagID]);
        }

        private Dialogue? GetDialogue(string name) => GetElement<Dialogue>(name);


        private Dialogue? GetDialogue(int id) => GetElement<Dialogue>(id);
    }
}