﻿using System;

namespace XVNML.Core.Tags
{
    public class TagBase : IResolvable
    {
        public string tagTypeName;
        public string tagName
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
        public TagParameterInfo parameterInfo;
        public List<TagBase> elements;
        public TagBase parentTag;
        public object value;
        public bool isSelfClosing = false;

        internal TagEvaluationState tagState;

        /// <summary>
        /// Will find the tag of type with a
        /// parameter called "name"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public T GetElement<T>(string tagName) where T : TagBase
        {
            T element = (T)elements.Find(e => e.tagName == tagName);
            return element;
        }

        /// <summary>
        /// Will find the first occurance of this tag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetElement<T>() where T : TagBase
        {
            T element = (T)elements.Find(e=> e.GetType() == typeof(T));
            return element;
        }

        /// <summary>
        /// Will find the index of this tag based on
        /// it's occurance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetElement<T>(int index) where T : TagBase
        {
            int i = 0;
            T element = (T)elements.Find(e => (e.GetType() == typeof(T) && i == index));
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
        public virtual void OnResolve()
        {
            var validTagType = DefinedTagsCollection.ValidTagTypes[tagTypeName];
            var config = validTagType.Item2;
            var existanceFlag = validTagType.Item3;

            //Check config pragmaonce
            if (config.PragmaOnce == true)
            {
                if (existanceFlag == true || (existanceFlag && config.DependingTag == parentTag.GetType().Name))
                {
                    Console.WriteLine("This tag already exists within the document. There can only be 1");
                    Parser.Parser.Abort();
                    return;
                }

                validTagType.Item3 = config.PragmaOnce;
            }

            //Check that the Parent tag matches the depending tag
            //If there is a parent tag, but doesn't match the depending tag
            if(parentTag != null && config.DependingTag != null && parentTag.GetType().Name != config.DependingTag)
            {
                Console.WriteLine($"Invalid Depending Tag. The tag {tagTypeName} depends on {config.DependingTag}");
                Parser.Parser.Abort();
                return;
            }

            //If there is no parent tag, but it depends on a tag
            if(parentTag == null && config.DependingTag != null)
            {
                Console.WriteLine($"The tag {tagTypeName} depends on {config.DependingTag}, but there is nothing.");
                Parser.Parser.Abort();
                return;
            }

            //Parser can continue at this point.
        }
    }
}