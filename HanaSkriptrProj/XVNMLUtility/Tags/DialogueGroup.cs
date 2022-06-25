using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("dialogueGroup", typeof(Proxy), TagOccurance.Multiple)]
    public class DialogueGroup : TagBase
    {
        public object? this[int index]
        {
            get { return GetDialogue(index).value; }
        }

        public new object? this[string name]
        {
            get { return GetDialogue(name).value; }
        }

        public Dialogue? GetDialogue(string name) => GetElement<Dialogue>(name);

        public Dialogue? GetDialogue(int id) => GetElement<Dialogue>(id);
    }
}
