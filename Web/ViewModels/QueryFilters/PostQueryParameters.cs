namespace Web.ViewModels.QueryFilters;

/// <summary>
///     Blog article query parameters
/// </summary>
public class PostQueryParameters : QueryParameters
{
    /// <summary>
    ///     Only request published articles
    /// </summary>
    public bool OnlyPublished { get; set; } = false;

    /// <summary>
    ///     Article status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    ///     Category ID
    /// </summary>
    public int CategoryId { get; set; } = 0;

    /// <summary>
    ///     Sorting field
    /// </summary>
    public new string? SortBy { get; set; } = "-LastUpdateTime";
}