namespace XVNML.Core.Dialogue.Enums
{
    public enum DialogueParserMode : sbyte
    {
        //If an @ is first, it's dialogue
        Dialogue,

        //If an ? is first, set up for dialogue prompts
        Prompt
    }
}
