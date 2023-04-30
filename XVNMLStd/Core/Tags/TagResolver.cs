using System;

namespace XVNML.Core.Tags
{
    internal class TagConverter
    {
        internal static TagBase? Convert(string text)
        {
            if (DefinedTagsCollection.ValidTagTypes?.ContainsKey(text) == false)
            {
                {
                    var msg = $"Error in Tag Resolver: There is no association with tag {text}";
                    Console.WriteLine(msg);
                    Parser.TagParser.Abort(msg);
                }
            }

            var tag = (TagBase?)Activator.CreateInstance(DefinedTagsCollection.ValidTagTypes?[text].Item1!);

            return tag;
        }
    }
}
