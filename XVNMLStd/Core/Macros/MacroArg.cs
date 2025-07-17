using System;
using XVNML.Core.Native;

namespace XVNML.Core.Macros
{
    public sealed class MacroArg<T> where T : new()
    {
        Type _argType = typeof(T);
        T _argValue = new T();
        string? _runtimeReferenceTag = null;

        public T Value
        {
            get
            {
                return _argValue;
            }
            set
            {
                if (RuntimeReferenceTable.Map.ContainsKey(value?.ToString()!))
                {
                    _argValue = (T)Convert.ChangeType(RuntimeReferenceTable.Get(value?.ToString()!), typeof(T));
                }

                _argValue = value;
            }
        }

        public MacroArg(T value)
        {
            _argValue = value;
        }

        public static implicit operator MacroArg<T>(T value)
        {
            return new MacroArg<T>(value);
        }
    }
}
