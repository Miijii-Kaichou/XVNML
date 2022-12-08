using System.Linq;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("castDefinitions", new[] { typeof(Proxy), typeof(Source) }, TagOccurance.PragmaOnce)]
    public sealed class CastDefinitions : TagBase
    {
        public Cast[]? CastMembers => Collect<Cast>();
        public Cast? this[string name]
        {
            get { return GetCast(name); }
        }

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
        }

        public Cast? GetCast(string name) => CastMembers.First(cast => cast.tagName?.Equals(name) == true);
    }
}
