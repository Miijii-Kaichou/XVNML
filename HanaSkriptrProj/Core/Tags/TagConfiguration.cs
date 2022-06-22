namespace XVNML.Core.Tags
{
    internal struct TagConfiguration
    {
        public string LinkedTag { get; internal set; }
        public string DependingTag { get; internal set; }
        public bool PragmaOnce { get; internal set; }
        public bool UserDefined { get; internal set; }
    }
}