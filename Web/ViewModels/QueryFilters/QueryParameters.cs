namespace Web.ViewModels.QueryFilters;

/// <summary>
///     Request Parameters
/// </summary>
public class QueryParameters
{
    /// <summary>
    ///     Maximum page size
    /// </summary>
    public const int MaxPageSize = 50;

    private int _pageSize = 10;

    /// <summary>
    ///     Page size
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    ///     Current page number
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    ///     Search keyword
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    ///     Sorting field
    /// </summary>
    public string? SortBy { get; set; }
}