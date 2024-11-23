namespace Web.ViewModels.QueryFilters;

/// <summary>
///     Blog article query parameters
/// </summary>
public class PostQueryParameters : QueryParameters
{
    /// <summary>
    /// Is Published (Only administrators can view and manage unpublished articles)
    /// </summary>
    public bool? IsPublish { get; set; } = null;

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