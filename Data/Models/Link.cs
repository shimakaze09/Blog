namespace Data.Models;

/// <summary>
///     Friend Links
/// </summary>
public class Link
{
    /// <summary>
    ///     Website Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Introduction
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Is Visible
    /// </summary>
    public bool Visible { get; set; }
}