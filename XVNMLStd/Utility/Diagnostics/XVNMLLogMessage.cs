namespace XVNML.Utility.Diagnostics
{
    public sealed class XVNMLLogMessage
    {
        public string? Message { get; internal set; }
        public object? Context { get; internal set; }
        public object? Blame { get; internal set; }
        public XVNMLLogLevel Level { get; internal set; }
    }
}