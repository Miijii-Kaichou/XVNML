using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using XVNML.Core.Enums;
using XVNML.Core.Extensions;
using XVNML.Core.Parser;
using XVNML.Core.Tags.UserOverrides;


namespace XVNML.Core.Tags
{
    public abstract class TagBase : IResolvable
    {
        protected TagFormRestrictionMode TagFormRestrictionMode { get; } = TagFormRestrictionMode.None;

        public string? _tagName;

        public string? TagTypeName
        {
            get
            {
                return GetType().Name;
            }
        }

        [JsonProperty]
        public string? TagName
        {
            get
            {
                var name = GetParameterValue<string>("name");
                if (name != null)
                {
                    return name;
                }

                _tagName ??= TagTypeName;

                return _tagName;
            }
            internal set
            {
                _tagName = value;
            }
        }

        private int? _tagId = null;
        [JsonProperty]
        public int? TagID
        {
            get
            {
                var id = GetParameterValue<string>("id");
                if (id != null)
                {
                    return Convert.ToInt32(id);
                }
                return _tagId;
            }
            internal set
            {
                _tagId = value;
            }
        }

        [JsonProperty]
        public string[]? AlternativeTagNames
        {
            get
            {
                var value = GetParameterValue<string>("altName");
                if (value == null) return null;
                return value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
        }

        [JsonProperty]
        protected virtual string[]? AllowedParameters { get; set; }

        [JsonProperty]
        protected virtual string[]? AllowedFlags { get; set; }

        public List<TagBase>? elements = null;
        public TagBase? parentTag = null;
        public object? value = new object();
        public bool isSelfClosing = false;
        public bool IsResolved { get; internal set; }
        public TagParser? ParserRef { get; internal set; }

        internal TagEvaluationState tagState;
        internal bool isSettingFlag;
        internal TagParameterInfo? _parameterInfo;

        private bool _allowParametersValidated = false;
        private bool _allowFlagsValidated = false;

        protected TagBase() { }

        private readonly string[] DefaultAllowedParameters = new string[3]
        {
            "name",
            "altNames",
            "id"
        };

        private readonly string[] DefaultAllowedFlags = new string[1]
        {
            "override"
        };

        public object? this[ReadOnlySpan<char> name]
        {
            get { return _parameterInfo?.GetParameter(name.ToString())?.value; }
        }

        /// <summary>
        /// Will perform a through search from this tag.
        /// If it can't find the name, it will return the first
        /// occurance of the tag with the matching type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tagName"></param>
        /// <remarks>Use this sparly, as it may hender performance.</remarks>
        public T? SearchElement<T>(string tagName) where T : TagBase
        {
            if (elements == null)
                return null;

            foreach (var currentChildElement in elements)
            {
                if (TagMatches(currentChildElement, tagName))
                    return (T?)currentChildElement;

                T? element = currentChildElement.SearchElement<T>(tagName);

                if (element != null)
                    return element;
            }

            return null;
        }

        private bool TagMatches(TagBase tag, string tagName)
        {
            if (tag.TagName == tagName)
                return true;

            if (tag.AlternativeTagNames != null && tag.AlternativeTagNames.Contains(tagName))
                return true;

            return false;
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
            T? element = (T?)elements.Find(e => e.TagName == tagName ||
                                                (e.AlternativeTagNames?.Length > 0 && e.AlternativeTagNames?.Contains(tagName) == true));
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
                int? id = (int?)e?._parameterInfo?["id"];
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

        public T GetParameterValue<T>(string parameterName)
        {
            var value = GetParameter(parameterName)?.value;

            if (value == null) return default!;

            if (typeof(T).IsEnum)
            {
                if (Enum.IsDefined(typeof(T), value))
                    return (T)Enum.Parse(typeof(T), value?.ToString());
                return default!;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public TagParameter? GetParameter(string parameterName)
        {
            if (_parameterInfo == null) return null;
            
            return _parameterInfo.GetParameter(parameterName);
        }

        public bool HasFlag(string flagName)
        {
            if (_parameterInfo == null) return false;
            
            return _parameterInfo.HasFlag(flagName);
        }

        /// <summary>
        /// Once a tag has been successfully resolved,
        /// use this method to assign a tag whatever value you need.
        /// </summary>
        public virtual void OnResolve(string? fileOrigin)
        {
            if (_allowParametersValidated == false) ValidateAllowedParameters();
            if (_allowFlagsValidated == false)      ValidateAllowedFlags();

            var validTagType = DefinedTagsCollection.ValidTagTypes![_tagName!];
            var config = validTagType.Item2;
            var appearanceLocation = validTagType.Item3;
            var currentFile = validTagType.Item4 ?? ParserRef!.fileTarget;
            var msg = string.Empty;

            if (config.TagOccurance == TagOccurance.PragmaLocalOnce)
            {
                if (appearanceLocation.Contains(parentTag!) && currentFile == ParserRef!.fileTarget)
                {
                    msg = $"This tag already exists within the scope {parentTag!.TagName}. There can only be 1: Tag Name {TagName}: {ParserRef.fileTarget}";
                    Console.WriteLine(msg);
                    Complain(msg);
                    return;
                }

                validTagType.Item3.Add(parentTag!);
                validTagType.Item4 = ParserRef!.fileTarget!;
                DefinedTagsCollection.ValidTagTypes[_tagName!] = validTagType;
            }

            if (config.TagOccurance == TagOccurance.PragmaOnce)
            {
                if (appearanceLocation.Count > 0 && currentFile == ParserRef!.fileTarget)
                {
                    msg = $"This tag already exists within the document. There can only be 1: Tag Name {TagName}: {ParserRef.fileTarget}";
                    Console.WriteLine(msg);
                    Complain(msg);
                    return;
                }

                validTagType.Item3.Add(parentTag!);
                validTagType.Item4 = ParserRef!.fileTarget!;
                DefinedTagsCollection.ValidTagTypes[_tagName!] = validTagType;
            }

            //Check that the Parent tag matches the depending tag
            //If there is a parent tag, but doesn't match the depending tag
            if (parentTag != null && config.DependingTags != null && config.DependingTags.Contains(parentTag.GetType().Name) == false)
            {
                msg = $"Invalid Depending Tag {parentTag}. The tag {_tagName} depends on {config.DependingTags.JoinStringArray()}: {ParserRef!.fileTarget}";
                Console.WriteLine(msg);
                Complain(msg);
                return;
            }

            //If there is no parent tag, but it depends on a tag
            if (parentTag == null && config.DependingTags != null)
            {
                msg = $"The tag {_tagName} depends on {config.DependingTags}, but there is nothing.: {ParserRef!.fileTarget}";
                Console.WriteLine(msg);
                Complain(msg);
                return;
            }

            IsResolved = true;
        }

        protected T[] Collect<T>() where T : TagBase
        {
            int id = 0;
            Construct<List<T>>(out var list);
            elements?.ForEach(item =>
            {
                if (item == null) return;
                if (item.GetType() != typeof(T)) return;
                if (item.TagID == null)
                {
                    item.TagID = id++;
                    list.Add((T)Convert.ChangeType(item, typeof(T)));
                    return;
                }

                id = item.TagID.Value;
                list.Add((T)Convert.ChangeType(item, typeof(T)));
            });
            return list.ToArray();
        }

        public static TInput Construct<TInput>(out TInput result) where TInput : new()
        {
            return result = new TInput();
        }

        private void ValidateAllowedParameters()
        {
            if (_parameterInfo == null) return;

            _allowParametersValidated = true;

            if (AllowedParameters == null
            || AllowedParameters.Length == 0)
            {
                AllowedParameters = DefaultAllowedParameters;
            }

            for (int i = 0; i < _parameterInfo!.parameters.Keys.Count; i++)
            {
                string currentSymbol = _parameterInfo.parameters.Keys.ToArray()[i];

                if (AllowedParameters.Contains(currentSymbol) == false
                && DefaultAllowedParameters.Contains(currentSymbol) == false)
                {
                    Type instanceType = GetType();
                    if (_parameterInfo!.flagParameters.Contains(DefaultAllowedFlags.First())
                    && UserOverrideManager.AnyParameterOverrides
                    && IsTagSubjectedToParameterOverride(currentSymbol, instanceType))
                        continue;

                    Complain($"{currentSymbol} is not an allowed parameter" +
                    $" for tag \"{TagName}\" (typeof \"{TagTypeName}\")");

                    return;
                }
            }
        }

        private void ValidateAllowedFlags()
        {
            if (_parameterInfo == null) return;

            _allowFlagsValidated = true;

            if (AllowedFlags == null
            || AllowedFlags.Length == 0)
            {
                AllowedFlags = DefaultAllowedFlags;
            }

            for (int i = 0; i < _parameterInfo!.flagParameters.Count; i++)
            {
                string currentSymbol = _parameterInfo.flagParameters[i];
                
                if (AllowedFlags.Contains(currentSymbol) == false
                && DefaultAllowedFlags.Contains(currentSymbol) == false)
                {
                    Type instanceType = GetType();
                    if (_parameterInfo!.flagParameters.Contains(DefaultAllowedFlags.First())
                    && UserOverrideManager.AnyFlagOverrides
                    && IsTagSubjectedToFlagOverride(currentSymbol, instanceType))
                        continue;

                    Complain($"{currentSymbol} is not an allowed flag" +
                    $" for tag \"{TagName}\" (typeof \"{TagTypeName}\")");

                    return;
                }
            }
            _allowFlagsValidated = true;
        }


        private bool IsTagSubjectedToParameterOverride(string? currentSymbol, Type instanceType)
        {
            if (UserOverrideManager.AllowParameters.ContainsKey((instanceType, TagName)) == false)
            {
                bool DoesGlobalParameterOverrideExists = UserOverrideManager.AllowParameters.ContainsKey((instanceType, instanceType.Name));
                if (DoesGlobalParameterOverrideExists == false) return DoesGlobalParameterOverrideExists;
                return UserOverrideManager.AllowParameters[(instanceType, instanceType.Name)]?.Contains(currentSymbol!) == DoesGlobalParameterOverrideExists;
            }

            return UserOverrideManager.AllowParameters[(instanceType, TagName)]?.Contains(currentSymbol!) == true;
        }

        private bool IsTagSubjectedToFlagOverride(string? currentSymbol, Type instanceType)
        {
            if (UserOverrideManager.AllowFlags.ContainsKey((instanceType, TagName)) == false)
            {
                bool DoesGlobalFlagParameterOverrideExists = UserOverrideManager.AllowFlags.ContainsKey((instanceType, instanceType.Name));
                if (DoesGlobalFlagParameterOverrideExists == false) return DoesGlobalFlagParameterOverrideExists;
                return UserOverrideManager.AllowFlags[(instanceType, instanceType.Name)]?.Contains(currentSymbol!) == DoesGlobalFlagParameterOverrideExists;
            }

            return UserOverrideManager.AllowFlags[(instanceType, TagName)]?.Contains(currentSymbol!) == true;
        }

        private void Complain(string msg)
        {
            TagParser.Abort(msg);
        }
    }
}