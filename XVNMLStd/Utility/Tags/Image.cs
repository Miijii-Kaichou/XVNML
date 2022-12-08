using System.IO;
using XVNML.Core.Tags;

namespace XVNML.XVNMLUtility.Tags
{
    [AssociateWithTag("image", typeof(ImageDefinitions), TagOccurance.Multiple)]
    public sealed class Image : TagBase
    {
        const string _CastDir = @"\Images\";
        internal DirectoryInfo? dirInfo;

        public override void OnResolve(string? fileOrigin)
        {
            base.OnResolve(fileOrigin);
            string src = (string?)parameterInfo?["src"] ?? string.Empty;
            dirInfo = new DirectoryInfo(fileOrigin + _CastDir + src);
        }

        public override string ToString()
        {
            return dirInfo == null ? string.Empty : dirInfo!.FullName;
        }
    }
}