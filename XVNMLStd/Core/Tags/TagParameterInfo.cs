using System;
using System.Collections.Generic;

namespace XVNML.Core.Tags
{
    public class TagParameterInfo
    {
        internal Dictionary<string, TagParameter> parameters = new Dictionary<string, TagParameter>();
        internal List<string> flagParameters = new List<string>();

        internal int totalParameters => parameters.Count;

        internal TagParameter? GetParameter(string name)
        {
            if (parameters.ContainsKey(name) == false) return null;
            return parameters[name];
        }
        internal bool HasFlag(string name) => flagParameters.Contains(name);

        public object? this[string? name]
        {
            get
            {
                if (parameters.ContainsKey(name!) == false) return null;
                return parameters[name!].value;
            }
        }
    }

    public class TagParameter
    {
        internal string? name;
        internal object? value;
        internal bool isReferencing = false;
        internal Type? type => value?.GetType();
    }
}