using System.Text;

namespace XVNML.Core.Extensions
{
    internal static class ListExtensions
    {
        internal static string JoinStringArray(this string[] array)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in array)
            {
                sb.Append(item);
            }
            return sb.ToString();
        }
    }
}
