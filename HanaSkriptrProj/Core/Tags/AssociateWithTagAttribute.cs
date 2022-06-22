namespace XVNML.Core.Tags
{
    public enum TagOccurance
    {
        Once,
        Multiple
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AssociateWithTagAttribute : Attribute
    {
        public string Tag { get; }
        public Type ParentTag { get; }
        public TagOccurance Occurance { get; }
        public bool IsUserDefined { get; }

        /// <summary>
        /// Associate a C# class with a tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        public AssociateWithTagAttribute(string tag)
        {
            Tag = tag;
        }

        /// <summary>
        /// Associate a C# class with a tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        /// <param name="isUserDefined">Is this tag defined by the user?</param>
        /// <remarks>Telling that the class is user Defined enables extensibility of XVNML
        /// beyond what tags it already provides you.</remarks>
        public AssociateWithTagAttribute(string tag, bool isUserDefined)
        {
            Tag = tag;
            IsUserDefined = isUserDefined;
        }

        /// <summary>
        /// Associate a C# class with a tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        /// <param name="occurance">If the tag should only appear once in your document.</param>
        /// <remarks>The tag will occur only once by default. If you plan to use multiple,
        /// set the occurance to TagOccurance.Multiple</remarks>
        public AssociateWithTagAttribute(string tag, TagOccurance occurance)
        {
            Tag = tag;
            Occurance = occurance;
        }

        /// <summary>
        /// Associate a C# class with a tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        /// <param name="occurance">If the tag should only appear once in your document.</param>
        /// <param name="isUserDefined">Is this tag defined by the user?</param>
        public AssociateWithTagAttribute(string tag, TagOccurance occurance, bool isUserDefined)
        {
            Tag = tag;
            Occurance = occurance;
            IsUserDefined = isUserDefined;
        }

        /// <summary>
        /// Associate a C# class with a tag that is to be found with a parent tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        /// <param name="parentTag">The parent tag in which you'll find this tag in.</param>
        /// <param name="occurance"></param>
        /// <param name="isUserDefined">Is this tag defined by the user?</param>
        /// <remarks>Type must be or derive from type TagBase.</remarks>
        public AssociateWithTagAttribute(string tag, Type parentTag, TagOccurance occurance, bool isUserDefined)
        {
            Tag = tag;
            ParentTag = parentTag;
            Occurance = occurance;
            IsUserDefined = isUserDefined;
        }

        /// <summary>
        /// Associate a C# class with a tag that is to be found with a parent tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        /// <param name="parentTag">The parent tag in which you'll find this tag in.</param>
        /// <param name="occurance">If the tag should only appear once in your document.</param>
        /// <remarks>Type must be or derive from type TagBase.</remarks>
        public AssociateWithTagAttribute(string tag, Type parentTag, TagOccurance occurance)
        {
            Tag = tag;
            ParentTag = parentTag;
            Occurance = occurance;
        }

        /// <summary>
        /// Associate a C# class with a tag that is to be found with a parent tag
        /// </summary>
        /// <param name="tag">The target tag to link the class with.</param>
        /// <param name="parentTag">The parent tag in which you'll find this tag in.</param>
        /// <param name="isUserDefined">Is this tag defined by the user?</param>
        /// <remarks>Type must be or derive from type TagBase.</remarks>
        public AssociateWithTagAttribute(string tag, Type parentTag, bool isUserDefined)
        {
            Tag = tag;
            ParentTag = parentTag;
            IsUserDefined = isUserDefined;
        }
    }
}
