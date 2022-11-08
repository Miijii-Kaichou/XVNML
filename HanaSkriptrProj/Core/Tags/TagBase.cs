﻿using XVNML.Core.Extensions;
using OriginParser = XVNML.Core.Parser.Parser;
namespace XVNML.Core.Tags
{
    public class TagBase : IResolvable
    {
        public string? tagTypeName;
        public string? tagName
        {
            get
            {
                var name = GetParameterValue("name")?.ToString();
                if (name != null)
                {
                    return name;
                }

                return tagTypeName;
            }
        }
        public TagParameterInfo? parameterInfo;
        public List<TagBase>? elements;
        public TagBase? parentTag;
        public object? value;
        public bool isSelfClosing = false;
        internal OriginParser parserRef;

        internal TagEvaluationState tagState;

        public object this[ReadOnlySpan<char> name]
        {
            get { return parameterInfo?[name.ToString()] ?? new object(); }
        }

        /// <summary>
        /// Will find the tag of type with a
        /// parameter called "name"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public T? GetElement<T>(string tagName) where T : TagBase
        {
            if (elements == null) return null;
            T? element = (T?)elements.Find(e => e.tagName == tagName);
            return element;
        }

        /// <summary>
        /// Will find the first occurance of this tag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetElement<T>() where T : TagBase
        {
            if (elements == null) return null;
            T? element = (T?)elements.Find(e => e.GetType() == typeof(T));
            return element;
        }

        /// <summary>
        /// Will find the index of this tag based on
        /// it's occurance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? GetElement<T>(int index) where T : TagBase
        {
            if (elements == null) return null;
            int i = 0;

            //Check if id's are already provided
            foreach (T? e in elements)
            {
                int? id = (int?)e?.parameterInfo?["id"];
                if (id == null) continue;
                if (id == index) return e;
            }

            //Then we do normal Find method
            T? element = (T?)elements.Find(e => e.GetType() == typeof(T) && i == index);
            return element;
        }

        public object? GetParameterValue(string parameterName)
        {
            try
            {
                if (parameterInfo == null) return parameterInfo;

                TagParameter? parameter = parameterInfo.GetParameter(parameterName);
                return parameter?.value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Once a tag has been successfully resolved,
        /// use this method to assign a tag whatever value you need.
        /// </summary>
        internal virtual void OnResolve(string fileOrigin)
        {
            var validTagType = DefinedTagsCollection.ValidTagTypes![tagTypeName!];
            var config = validTagType.Item2;
            var appearanceLocation = validTagType.Item3;
            var currentFile = validTagType.Item4 ?? OriginParser.FileTarget;

            if (config.TagOccurance == TagOccurance.PragmaLocalOnce)
            {
                if (appearanceLocation.Contains(parentTag!) && currentFile == OriginParser.FileTarget)
                {
                    Console.WriteLine($"This tag already exists within the scope {parentTag!.tagName}. There can only be 1: Tag Name {tagName}: {OriginParser.FileTarget}");
                    OriginParser.Abort();

                    return;
                }

                validTagType.Item3.Add(parentTag!);
                validTagType.Item4 = OriginParser.FileTarget;
                DefinedTagsCollection.ValidTagTypes[tagTypeName!] = validTagType;
            }

            if (config.TagOccurance == TagOccurance.PragmaOnce)
            {
                if (appearanceLocation.Count > 0 && currentFile == OriginParser.FileTarget)
                {
                    Console.WriteLine($"This tag already exists within the document. There can only be 1: Tag Name {tagName}: {OriginParser.FileTarget}");
                    OriginParser.Abort();
                    return;
                }

                validTagType.Item3.Add(parentTag!);
                validTagType.Item4 = OriginParser.FileTarget;
                DefinedTagsCollection.ValidTagTypes[tagTypeName!] = validTagType;
            }

            //Check that the Parent tag matches the depending tag
            //If there is a parent tag, but doesn't match the depending tag
            if (parentTag != null && config.DependingTags != null && config.DependingTags.Contains(parentTag.GetType().Name) == false)
            {
                Console.WriteLine($"Invalid Depending Tag {parentTag}. The tag {tagTypeName} depends on {config.DependingTags.JoinStringArray()}: {OriginParser.FileTarget}");
                Parser.Parser.Abort();
                return;
            }

            //If there is no parent tag, but it depends on a tag
            if (parentTag == null && config.DependingTags != null)
            {
                Console.WriteLine($"The tag {tagTypeName} depends on {config.DependingTags}, but there is nothing.: {OriginParser.FileTarget}");
                Parser.Parser.Abort();
                return;
            }
        }

        protected T[] Collect<T>()
        {
            Construct<List<T>>(out var list);
            elements?.ForEach(item => list.Add((T)Convert.ChangeType(item, typeof(T)))); 
            return list.ToArray();
        }

        public static TInput Construct<TInput>(out TInput result) where TInput : new()
        {
            return (result = new TInput());
        }
    }
}