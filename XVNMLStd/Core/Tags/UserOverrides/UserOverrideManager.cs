using System;
using System.Collections.Generic;

namespace XVNML.Core.Tags.UserOverrides
{
    /// <summary>
    /// Overrides to include inside pre-built tags prior to XVNML Parsing.
    /// </summary>
    public static class UserOverrideManager
    {
        public static Dictionary<(Type tagType, string? identifier), List<string>> AllowParameters  { get; internal set; } 
            = new Dictionary<(Type tagType, string? identifier), List<string>>();

        public static Dictionary<(Type tagType, string? identifier), List<string>> AllowFlags       { get; internal set; } 
            = new Dictionary<(Type tagType, string? identifier), List<string>>();

        public static bool AnyParameterOverrides    => AllowParameters.Count != 0;
        public static bool AnyFlagOverrides         => AllowFlags.Count != 0;

        public static void IncludeAsAllowedParameter<T>(string newParameter) where T : TagBase, new()
        {
            IncludeAsAllowedParameter<T>(typeof(T).Name, newParameter);
        }

        public static void IncludeAsAllowedParameter<T>(string? tagIdentifier, string newParameter) where T : TagBase
        {
            if (AllowParameters.ContainsKey((typeof(T), tagIdentifier)))
            {
                AllowParameters[(typeof(T), tagIdentifier)].Add(newParameter);
                return;
            }

            AllowParameters.Add((typeof(T), tagIdentifier), new List<string> { newParameter });
        }

        public static void IncludeAsAllowedFlag<T>(string newFlag) where T : TagBase, new()
        {
            IncludeAsAllowedFlag<T>(typeof(T).Name, newFlag);
        }

        public static void IncludeAsAllowedFlag<T>(string? tagIdentifier, string newFlag) where T : TagBase
        {
            if (AllowFlags.ContainsKey((typeof(T), tagIdentifier)))
            {
                AllowFlags[(typeof(T), tagIdentifier)].Add(newFlag);
                return;
            }
                
            AllowFlags.Add((typeof(T), tagIdentifier), new List<string> { newFlag });
        }
    }
}
