using System;
using System.Collections.Generic;
using System.Linq;
using XVNML.Core.Extensions;
using XVNML.Core.TagParser;

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

        public string[]? alternativeTagNames
        {
            get
            {
                return GetParameterValue("altName")?.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public TagParameterInfo? parameterInfo;
        public List<TagBase>? elements;
        public TagBase? parentTag;
        public object? value;
        public bool isSelfClosing = false;
        internal Parser? parserRef;

        internal TagEvaluationState tagState;
        internal bool isSettingFlag;

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
            T? element = (T?)elements.Find(e => e.tagName == tagName ||
                                                (e.alternativeTagNames?.Length > 0 && e.alternativeTagNames?.Contains(tagName) == true));
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
            T? element = (T?)elements.Find(e =>
            {
                var condition = e.GetType() == typeof(T) && i == index;
                i++;
                return condition;
            });
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
        public virtual void OnResolve(string? fileOrigin)
        {
            var validTagType = DefinedTagsCollection.ValidTagTypes![tagTypeName!];
            var config = validTagType.Item2;
            var appearanceLocation = validTagType.Item3;
            var currentFile = validTagType.Item4 ?? parserRef!.fileTarget;
            var msg = string.Empty;

            if (config.TagOccurance == TagOccurance.PragmaLocalOnce)
            {
                if (appearanceLocation.Contains(parentTag!) && currentFile == parserRef!.fileTarget)
                {
                    msg = $"This tag already exists within the scope {parentTag!.tagName}. There can only be 1: Tag Name {tagName}: {parserRef.fileTarget}";
                    Console.WriteLine(msg);
                    Parser.Abort(msg);

                    return;
                }

                validTagType.Item3.Add(parentTag!);
                validTagType.Item4 = parserRef!.fileTarget!;
                DefinedTagsCollection.ValidTagTypes[tagTypeName!] = validTagType;
            }

            if (config.TagOccurance == TagOccurance.PragmaOnce)
            {
                if (appearanceLocation.Count > 0 && currentFile == parserRef!.fileTarget)
                {
                    msg = $"This tag already exists within the document. There can only be 1: Tag Name {tagName}: {parserRef.fileTarget}";
                    Console.WriteLine(msg);
                    Parser.Abort(msg);
                    return;
                }

                validTagType.Item3.Add(parentTag!);
                validTagType.Item4 = parserRef!.fileTarget!;
                DefinedTagsCollection.ValidTagTypes[tagTypeName!] = validTagType;
            }

            //Check that the Parent tag matches the depending tag
            //If there is a parent tag, but doesn't match the depending tag
            if (parentTag != null && config.DependingTags != null && config.DependingTags.Contains(parentTag.GetType().Name) == false)
            {
                msg = $"Invalid Depending Tag {parentTag}. The tag {tagTypeName} depends on {config.DependingTags.JoinStringArray()}: {parserRef!.fileTarget}";
                Console.WriteLine(msg);
                TagParser.Parser.Abort(msg);
                return;
            }

            //If there is no parent tag, but it depends on a tag
            if (parentTag == null && config.DependingTags != null)
            {
                msg = $"The tag {tagTypeName} depends on {config.DependingTags}, but there is nothing.: {parserRef!.fileTarget}";
                Console.WriteLine(msg);
                TagParser.Parser.Abort(msg);
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
            return result = new TInput();
        }
    }
}