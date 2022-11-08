using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("tags", typeof(Metadata), TagOccurance.PragmaOnce)]
    sealed class Tags : TagBase
    {
        public string[]? list;
        internal override void OnResolve(string fileOrigin)
        {
            base.OnResolve(fileOrigin);
            char[] delimiters = new[] { ',', ' ', '\r', '\n' };
            var data = parameterInfo?["list"]?.ToString()?.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            list = data;
        }
        public bool IncludesTag(string tagName) => list.Contains(tagName);
    }
}
