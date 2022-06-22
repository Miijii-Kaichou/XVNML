using System.Reflection;
using System.Linq;

namespace XVNML.Core.Tags
{
    /// <summary>
    /// This keeps track of the kinds of Tag type exists
    /// and that includes custom ones / user-defined ones.
    /// It'll validate the usage of a tag using this.
    /// </summary>
    internal static class DefinedTagsCollection
    {
        public static SortedDictionary<string, (Type, TagConfiguration, bool)> ValidTagTypes;

        private static Assembly Assembly;
        
        public static void ManifestTagTypes()
        {
            Assembly = Assembly.GetExecutingAssembly();
            ValidTagTypes = new();

            //Find all objects that derive from TagBase
            Type[] tagBasesTypes = Assembly.GetTypes().Where(t => t.IsClass && t.IsSubclassOf(typeof(TagBase))).ToArray();
            
            //Get Assemblies
            foreach(Type type in tagBasesTypes)
            {
                if(type.CustomAttributes.Any() == false)
                {
                    Console.WriteLine($"Error in Tag Type Manifest. Must use AssociateWithTag attribute.");
                    return;
                }

                AssociateWithTagAttribute? attribute = type.GetCustomAttribute<AssociateWithTagAttribute>();

                if(attribute == null)
                {
                    return;
                }

                var tagConfig = new TagConfiguration
                {
                    LinkedTag = attribute.Tag,
                    DependingTag = attribute.ParentTag?.Name,
                    PragmaOnce = attribute.Occurance == TagOccurance.Once ? true : false,
                    UserDefined = attribute.IsUserDefined
                };

                //Add to validated tagTypes. This means when
                //parsing the XVNML, this type will be resolved.
                ValidTagTypes.Add(attribute.Tag, (type, tagConfig, false));
            }

            var tagCount = ValidTagTypes.Count;
            Console.WriteLine($"Existing tags: {tagCount} tags");
        }
    }
}
