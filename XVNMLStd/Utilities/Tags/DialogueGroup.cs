using Newtonsoft.Json;
using System;
using XVNML.Core.Tags;

using static XVNML.FlagConstants;

namespace XVNML.Utilities.Tags
{
    [AssociateWithTag("dialogueGroup", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.Multiple)]
    public sealed class DialogueGroup : TagBase
    {
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
        }

        private Dialogue? GetDialogue(string name) => GetElement<Dialogue>(name);


        private Dialogue? GetDialogue(int id) => GetElement<Dialogue>(id);
    }
}