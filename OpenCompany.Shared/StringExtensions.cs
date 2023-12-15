using System.IO;

namespace OpenCompany.Shared;

public static class StringExtensions
{
    public static string LowercaseFirstChar(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        var a = str.ToCharArray();
        a[0] = char.ToLower(a[0]);
        return new string(a);
    }
    
    public static string PathCombine(this string str, string otherStr)
    {
        return Path.Combine(str, otherStr)
            .Replace("\\", "/")
            .Replace("\"", "");
    }
}