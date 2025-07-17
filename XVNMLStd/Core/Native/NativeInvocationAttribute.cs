using System;
using System.Runtime.InteropServices;

namespace XVNML.Core.Native
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class NativeInvocationAttribute : Attribute
    {
        public string? EntryPoint;
        public CallingConvention CallingConvention;
        public NativeInvocationAttribute() { }
    }
}
