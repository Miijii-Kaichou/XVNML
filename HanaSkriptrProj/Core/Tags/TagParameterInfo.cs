namespace XVNML.Core.Tags
{
    public class TagParameterInfo
    {
        internal Dictionary<string, TagParameter> paramters = new();
        internal List<string> flagParameters = new();

        internal int totalParameters => paramters.Count;

        internal TagParameter GetParameter(string name) => paramters[name];

        public object? this[string name]
        {
            get
            {
                if (paramters.ContainsKey(name) == false) return null;
                return paramters[name].value;
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