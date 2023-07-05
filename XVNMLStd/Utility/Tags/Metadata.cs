using Newtonsoft.Json;
using XVNML.Core.Tags;
namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("metadata", TagOccurance.PragmaOnce)]
    public sealed class Metadata : TagBase
    {
        [JsonProperty] public Title? title;
        [JsonProperty] public Author? author;
        [JsonProperty] public Date? date;
        [JsonProperty] public Description? description;
        [JsonProperty] public Copyright? copyright;
        [JsonProperty] public Url? url;
        [JsonProperty] public Tags? tags;

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
