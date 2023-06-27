using XVNML.Core.Tags;

using static XVNML.Constants;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("description", typeof(Metadata), TagOccurance.PragmaOnce)]
    public sealed class Description : TagBase
    {
        public string? content;
        public override void OnResolve(string? fileOrigin)
        {
            AllowedParameters = new[]
            {
                ContentParameterString
            };

            base.OnResolve(fileOrigin);

            content = value?.ToString();
        }
    }
}
