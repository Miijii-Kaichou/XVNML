using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    //The "..." for associating tag will be irrelevant
    //This is something that is abstract, and can't be used
    //Inside a document
    //Because only Letter Characters can be parsed for tag names.
    //You'll just get an error if you type ... in as a tag name
    [AssociateWithTag("...", TagOccurance.Multiple, true)]
    public class UserDefined : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }
    }
}
