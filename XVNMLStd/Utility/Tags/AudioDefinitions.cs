using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("audioDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class AudioDefinitions : TagBase
    {
        public Audio[]? AudioCollection => Collect<Audio>();
        public Audio? this[string name]
        {
            get { return GetCast(name.ToString()); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        Audio? GetCast(string name) => AudioCollection.First(audio => audio.TagName?.Equals(name) == true);
    }
}
