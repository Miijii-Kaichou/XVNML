namespace XVNML.Core.Dialogue.Structs
{
    internal struct MacroBlockInfo
    {
        internal int blockPosition;
        internal (string macroSymbol, object[] args)[] macroCalls;
        internal void Initialize(int size)
        {
            macroCalls = new (string macroSymbol, object[] args)[size];
        }
        internal void SetPosition(int position)
        {
            blockPosition = position;
        }
    }
}