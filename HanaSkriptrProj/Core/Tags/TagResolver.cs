namespace XVNML.Core.Tags
{
    internal class TagConverter
    {
        internal static TagBase Convert(string text)
        {
            if (DefinedTagsCollection.ValidTagTypes.ContainsKey(text) == false)
            {
                {
                    Console.WriteLine($"Error in Tag Resolver: There is no association with tag {text}");
                    Parser.Parser.Abort();
                }
            }

            var config = DefinedTagsCollection.ValidTagTypes[text].Item2;
            var existanceFlag = DefinedTagsCollection.ValidTagTypes[text].Item3;

            var tag = (TagBase)Activator.CreateInstance(DefinedTagsCollection.ValidTagTypes[text].Item1);

            return tag;
        }
    }
}
