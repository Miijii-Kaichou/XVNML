using System;
using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogueGroup", typeof(Proxy), TagOccurance.Multiple)]
    public sealed class DialogueGroup : TagBase
    {
        public bool IsActingAsSceneController { get; private set; }

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
            AllowedFlags = new[]
            {
                ActAsSceneControllerFlagString,
                AllowOverrideFlagString
            };

            base.OnResolve(fileOrigin);

            // Flags
            IsActingAsSceneController = HasFlag(AllowedFlags[0]);
        }

        private Dialogue? GetDialogue(string name) => GetElement<Dialogue>(name);


        private Dialogue? GetDialogue(int id) => GetElement<Dialogue>(id);
    }
}