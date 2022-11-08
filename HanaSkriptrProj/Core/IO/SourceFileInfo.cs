using XVNML.Core.IO.Enums;

namespace XVNML.Core.IO
{
    internal sealed class SourceFileInfo
    {
        private readonly object? src;
        private readonly DirectoryRelativity relativity;
        private readonly string? pathString;

        public SourceFileInfo(object? src, DirectoryRelativity relativity)
        {
            this.src = src;
            this.relativity = relativity;
        }
    }
}
