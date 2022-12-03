using XVNML.Core.IO.Enums;

namespace XVNML.Core.IO
{
    public sealed class SourceFileInfo
    {
        private readonly object? src;
        private readonly DirectoryRelativity relativity;

        public SourceFileInfo(object? src, DirectoryRelativity relativity)
        {
            this.src = src;
            this.relativity = relativity;
        }
    }
}
