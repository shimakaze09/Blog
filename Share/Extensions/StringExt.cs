using Share.Utils;

namespace Share.Extensions;

public static class StringExt
{
    public static string Limit(this string str, int length)
    {
        return str.Length <= length ? str : str[..length];
    }

    /// <summary>
    ///     Limits the display length of a string and adds ellipsis at the end
    /// </summary>
    public static string LimitWithEllipsis(this string str, int length)
    {
        return str.Length <= length ? str : $"{str[..length]}...";
    }

    public static string ToSHA256(this string source)
    {
        return HashUtils.ComputeSHA256Hash(source);
    }

    public static string ToSHA384(this string source)
    {
        return HashUtils.ComputeSHA384Hash(source);
    }

    public static string ToSHA512(this string source)
    {
        return HashUtils.ComputeSHA512Hash(source);
    }
}