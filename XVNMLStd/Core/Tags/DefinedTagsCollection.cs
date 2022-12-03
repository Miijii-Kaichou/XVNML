using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XVNML.Core.Extensions;

namespace XVNML.Core.Tags
{
    /// <summary>
    /// This keeps track of the kinds of Tag type exists
    /// and that includes custom ones / user-defined ones.
    /// It'll validate the usage of a tag using this.
    /// </summary>
    public static class DefinedTagsCollection
    {
        public static SortedDictionary<string, (Type, TagConfiguration, List<TagBase>, string)>? ValidTagTypes;

        public static int ExistingTags { get; private set; }

        public static void ManifestTagTypes()
        {
            ValidTagTypes = new SortedDictionary<string, (Type, TagConfiguration, List<TagBase>, string)>();

            //Find all objects that derive from TagBase
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> tagBasesTypes = new List<Type>();
            foreach (var asm in assemblies)
            {
                foreach (var type in asm.
                                GetTypes().
                                Where(t => t.IsClass && t.IsSubclassOf(typeof(TagBase))).
                                ToArray()) tagBasesTypes.Add(type);
            }

            //Get Assemblies
            foreach (Type type in tagBasesTypes)
            {
                if (type.CustomAttributes.Any() == false)
                {
                    Console.WriteLine($"Error in Tag Type Manifest. Must use AssociateWithTag attribute.");
                    return;
                }

                AssociateWithTagAttribute? attribute = (AssociateWithTagAttribute)Convert.ChangeType(type.GetCustomAttribute(typeof(AssociateWithTagAttribute)), typeof(AssociateWithTagAttribute));

                if (attribute == null)
                {
                    return;
                }

                var tagConfig = new TagConfiguration
                {
                    LinkedTag = attribute.Tag,
                    DependingTags = attribute.ParentTags?.Names(),
                    TagOccurance = attribute.Occurance,
                    UserDefined = attribute.IsUserDefined
                };

                //Add to validated tagTypes. This means when
                //parsing the XVNML, this type will be resolved.
                ValidTagTypes.Add(attribute.Tag, (type, tagConfig, new List<TagBase>(), null)!);
            }

            ExistingTags = ValidTagTypes.Count;
        }
    }
}
