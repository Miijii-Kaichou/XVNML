namespace XVNML.Core.Parser.Enums
{
    //Parser State
    public enum ParserEvaluationState
    {
        //Normal Parsing of XVNML tag
        Tag,

        //Set conditions before following through with the parsing
        Preprocessing,

        //References, anything starting with a dollarsign $
        Referencing,

        //If a Dialogue Tag is defined
        //Parse the contents inside
        Dialogue,

        //After a tag being open
        //it'll evaluate anything between open and close tags
        TagValue,

        //If using the Script tag, parse the language
        //target. (not supported at this time)
        Script
    }
}