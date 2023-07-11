using System;

namespace XVNML.Core.Tags
{
    internal class TagConverter
    {
        internal static TagBase? ConvertToTagInstance(string text)
        {
            if (DefinedTagsCollection.ValidTagTypes == null) return null;

            if (!DefinedTagsCollection.ValidTagTypes.ContainsKey(text))
            {
                var msg = $"Error in Tag Resolver: There is no association with tag '{text}'";
                throw new ArgumentException(msg);
            }

            var tagType = DefinedTagsCollection.ValidTagTypes[text].Item1;
            if (tagType == null)
            {
                var msg = $"Error in Tag Resolver: Invalid tag type association for tag '{text}'";
                throw new InvalidOperationException(msg);
            }

            var constructor = tagType.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                var msg = $"Error in Tag Resolver: No parameterless constructor found for tag type '{tagType.FullName}'";
                throw new InvalidOperationException(msg);
            }

            var tagInstance = (TagBase)constructor.Invoke(null);
            return tagInstance;
        }
    }
}
