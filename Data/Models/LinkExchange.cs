namespace Data.Models;

/// <summary>
///     Link Exchange Record
/// </summary>
public class LinkExchange
{
    /// <summary>
    ///     Website Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Introduction
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Site Owner
    /// </summary>
    public string WebMaster { get; set; }

    /// <summary>
    ///     Contact Email
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    ///     Is Verified
    ///     <para>Link exchanges need verification before they can be displayed on the website.</para>
    /// </summary>
    public bool Verified { get; set; }

    /// <summary>
    ///     Application Time
    /// </summary>
    public DateTime ApplyTime { get; set; }
}