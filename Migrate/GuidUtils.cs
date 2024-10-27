namespace Migrate;

public static class GuidUtils
{
    /// <summary>
    /// 32-digit number separated by hyphens
    /// </summary>
    /// <returns></returns>
    private static string GetGuid()
    {
        var guid = new Guid();
        guid = Guid.NewGuid();
        return guid.ToString();
    }

    /// <summary>
    /// Get a 16-bit unique string based on the GUID
    /// </summary>
    /// <returns></returns>
    public static string GuidTo16String()
    {
        long i = 1;
        foreach (var b in Guid.NewGuid().ToByteArray())
        {
            i *= b + 1;
        }
        return $"{i - DateTime.Now.Ticks:x}";
    }

    /// <summary>
    /// Get a unique 19-digit sequence based on the GUID
    /// /// </summary>
    /// <returns></returns>
    public static long GuidToLongID()
    {
        var buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt64(buffer, 0);
    }
}