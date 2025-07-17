using System;

namespace XVNML.Core.Tags.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TagNamespaceAttribute : Attribute
    {
        public string Name { get; internal set; }
        
        public TagNamespaceAttribute(string namespaceName)
        {
            Name = namespaceName;
        }
    }
}
