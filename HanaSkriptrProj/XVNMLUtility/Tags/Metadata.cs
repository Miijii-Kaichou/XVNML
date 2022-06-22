using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("metadata", TagOccurance.Once)]
    public class Metadata : TagBase
    {
        public string? title;
        public string? author;
        public string? date;
        public string? description;
        public string? copyright;
        public string? url;
        public string? tags;

        public override void OnResolve()
        {
            base.OnResolve();

            if (parameterInfo == null) return;

            title = GetElement<Title>().parameterInfo["name"].ToString();
            author = GetElement<Author>().parameterInfo["name"].ToString();
            date = GetElement<Date>().parameterInfo["value"].ToString();
            description = GetElement<Description>().parameterInfo["text"].ToString();
            copyright = GetElement<Copyright>().parameterInfo["year"].ToString();
            url = GetElement<Url>().parameterInfo["href"].ToString();
            tags = GetElement<Tags>().parameterInfo["list"].ToString();
        }
    }
}
