using System;
using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogueGroup", typeof(Proxy), TagOccurance.Multiple)]
    public sealed class DialogueGroup : TagBase
    {
        public object? this[int index]
        {
            get { return GetDialogue(index)?.Script; }
        }

        public object? this[ReadOnlySpan<char> name]
        {
            get { return GetDialogue(name.ToString())?.Script; }
        }

        public Dialogue? GetDialogue(string name) => GetElement<Dialogue>(name);

        public Dialogue? GetDialogue(int id) => GetElement<Dialogue>(id);
    }
}
