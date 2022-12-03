namespace XVNML.Core.TagParser.Enums
{
    //Parse Resource State
    public enum ParseResourceState
    {
        Internal,

        //If a .xvnml is definied as a src for things
        //like scenes, casts, or audio,
        //It'll evaluate the contents of that files
        //and put information in the main .xvnml object
        External
    }
}