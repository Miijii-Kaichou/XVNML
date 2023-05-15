using System;
using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogueGroup", typeof(Proxy), TagOccurance.Multiple)]
    public sealed class DialogueGroup : TagBase
    {
        public bool IsActingAsSceneController { get; private set; }

        public object? this[int index]
        {
            get { return GetDialogue(index)?.Script; }
        }

        public new object? this[ReadOnlySpan<char> name]
        {
            get { return GetDialogue(name.ToString())?.Script; }
        }

        public override void OnResolve(string? fileOrigin)
        {
            AllowedFlags = new[]
            {
                "actAsSceneController",
                "allowOverride"
            };

            base.OnResolve(fileOrigin);

            // Flags
            IsActingAsSceneController = HasFlag(AllowedFlags[0]);
        }

        public Dialogue? GetDialogue(string name) => GetElement<Dialogue>(name);

        public Dialogue? GetDialogue(int id) => GetElement<Dialogue>(id);
    }
}
