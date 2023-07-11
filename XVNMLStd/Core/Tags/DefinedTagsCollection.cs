using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XVNML.Core.Assemblies;
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
        internal static SortedDictionary<string, (Type, TagConfiguration, List<TagBase>, string)>? ValidTagTypes;

        internal static int ExistingTags { get; private set; }

        public static bool IsInitialized { get; private set; } = false;

        internal static void ManifestTagTypes()
        {
            if (IsInitialized)
                return;

            ValidTagTypes = new SortedDictionary<string, (Type, TagConfiguration, List<TagBase>, string)>();

            // Filter and cache assemblies if possible

            List<Type> tagBaseTypes = new List<Type>();

            ExtractTagBaseTypes(tagBaseTypes);

            foreach (Type type in tagBaseTypes)
            {
                EstablishTagBaseAssociations(type, out bool hadConflict);
                if (hadConflict) return;
            }

            ExistingTags = ValidTagTypes.Count;

            IsInitialized = ExistingTags != 0;
        }

        private static void EstablishTagBaseAssociations(Type type, out bool conflict)
        {
            conflict = false;

            if (!type.CustomAttributes.Any())
            {
                Console.WriteLine($"Error in Tag Type Manifest. Must use AssociateWithTag attribute.");
                conflict = true;
                return;
            }

            AssociateWithTagAttribute? attribute = (AssociateWithTagAttribute)type.GetCustomAttribute(typeof(AssociateWithTagAttribute));

            if (attribute == null) return;

            var tagConfig = new TagConfiguration
            {
                LinkedTag = attribute.Tag,
                DependingTags = attribute.ParentTags?.Names(),
                TagOccurance = attribute.Occurance,
                UserDefined = attribute.IsUserDefined
            };

            ValidTagTypes?.Add(attribute.Tag, (type, tagConfig, new List<TagBase>(), null)!);
        }

        private static void ExtractTagBaseTypes(List<Type> tagBaseTypes)
        {
            var types = DomainAssemblyState.DefinedTypes!;
            foreach (var type in types)
            {
                if (type.IsClass && type.IsSubclassOf(typeof(TagBase)))
                    tagBaseTypes.Add(type);
            }
        }
    }
}
