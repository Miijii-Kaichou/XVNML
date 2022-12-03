using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    //The "..." for associating tag will be irrelevant
    //This is something that is abstract, and can't be used
    //Inside a document
    [AssociateWithTag("...", TagOccurance.Multiple, true)]
    public class UserDefined : TagBase
    {
        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

    }
}
