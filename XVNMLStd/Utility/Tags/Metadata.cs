using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("metadata", TagOccurance.PragmaOnce)]
    public sealed class Metadata : TagBase
    {
        public Title? title;
        public Author? author;
        public Date? date;
        public Description? description;
        public Copyright? copyright;
        public Url? url;
        public Tags? tags;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);

            title = GetElement<Title>();
            author = GetElement<Author>();
            date = GetElement<Date>();
            description = GetElement<Description>();
            copyright = GetElement<Copyright>();
            url = GetElement<Url>();
            tags = GetElement<Tags>();
        }
    }
}
