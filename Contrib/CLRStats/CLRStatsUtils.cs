namespace Contrib.CLRStats;

/// <summary>
/// Gets a plugin for monitoring .NET application resource usage, including CPU usage, GC, thread status, 
/// supports retrieving status information via web requests (can customize access path and authentication),
/// data will be returned in JSON format.
/// </summary>
public static class CLRStatsUtils
{
    private static readonly CLRStatsModel clrStatsModel = new CLRStatsModel();

    public static CLRStatsModel GetCurrentClrStats()
    {
        return clrStatsModel;
    }

    public static string GetCurrentClrStatsToJson()
    {
        return GetCurrentClrStats().ToJson();
    }
}
