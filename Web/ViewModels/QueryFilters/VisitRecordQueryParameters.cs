namespace Web.ViewModels.QueryFilters;

public class VisitRecordQueryParameters : QueryParameters
{
    /// <summary>
    /// Sorting field
    /// </summary>
    public new string? SortBy { get; set; } = "-Time";
}
