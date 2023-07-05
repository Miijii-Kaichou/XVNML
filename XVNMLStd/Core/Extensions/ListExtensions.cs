using System;
using System.Collections;
using System.Text;

namespace XVNML.Core.Extensions
{
    internal static class ListExtensions
    {
        internal static string JoinStringArray(this IEnumerable array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in array)
            {
                sb.Append(item.ToString()+Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
