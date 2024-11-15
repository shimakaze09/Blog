using Data.Models;
using FreeSql;
using Web.ViewModels.QueryFilters;
using X.PagedList;
using X.PagedList.Extensions;

namespace Web.Services;

public class VisitRecordService
{
    private readonly IBaseRepository<VisitRecord> _repo;

    public VisitRecordService(IBaseRepository<VisitRecord> repo)
    {
        _repo = repo;
    }

    /// <summary>
    ///     Retrieves a visit record by its ID.
    /// </summary>
    /// <param name="id">The ID of the visit record.</param>
    /// <returns>The visit record if found; otherwise, null.</returns>
    public async Task<VisitRecord?> GetById(int id)
    {
        var item = await _repo.Where(a => a.Id == id).FirstAsync();
        return item;
    }

    /// <summary>
    ///     Retrieves all visit records.
    /// </summary>
    /// <returns>A list of all visit records.</returns>
    public async Task<List<VisitRecord>> GetAll()
    {
        return await _repo.Select.OrderByDescending(a => a.Time).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a paginated list of visit records based on query parameters.
    /// </summary>
    /// <param name="param">The query parameters for filtering and pagination.</param>
    /// <returns>A paginated list of visit records.</returns>
    public async Task<IPagedList<VisitRecord>> GetPagedList(VisitRecordQueryParameters param)
    {
        var querySet = _repo.Select;

        // Search by request path
        if (!string.IsNullOrEmpty(param.Search)) querySet = querySet.Where(a => a.RequestPath.Contains(param.Search));

        // Sort by specified property
        if (!string.IsNullOrEmpty(param.SortBy))
        {
            // Determine if sorting is ascending
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');
            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        return (await querySet.ToListAsync()).ToPagedList(param.Page, param.PageSize);
    }

    /// <summary>
    ///     Retrieves an overview of visit data.
    /// </summary>
    /// <returns>An object containing total visits, today's visits, and yesterday's visits.</returns>
    public async Task<object> Overview()
    {
        return await _repo.Where(a => !a.RequestPath.StartsWith("/Api"))
            .ToAggregateAsync(g => new
            {
                TotalVisit = g.Count(),
                TodayVisit = g.Sum(g.Key.Time.Date == DateTime.Today ? 1 : 0),
                YesterdayVisit = g.Sum(g.Key.Time.Date == DateTime.Today.AddDays(-1).Date ? 1 : 0)
            });
    }

    /// <summary>
    ///     Retrieves trend data for the specified number of days.
    /// </summary>
    /// <param name="days">The number of days to view data for, default is 7 days.</param>
    /// <returns>A list of objects containing date and visit count for each day.</returns>
    public async Task<object> Trend(int days = 7)
    {
        return await _repo.Where(a => !a.RequestPath.StartsWith("/Api"))
            .Where(a => a.Time.Date > DateTime.Today.AddDays(-days).Date)
            .GroupBy(a => a.Time.Date)
            .ToListAsync(a => new
            {
                time = a.Key,
                date = $"{a.Key.Month}-{a.Key.Day}",
                count = a.Count()
            });
    }

    /// <summary>
    ///     Retrieves statistical data for a specific date.
    /// </summary>
    /// <param name="date">The date to retrieve statistics for.</param>
    /// <returns>An object containing the visit count for the specified date.</returns>
    public async Task<object> Stats(DateTime date)
    {
        return await _repo.Where(
            a => a.Time.Date == date.Date
                 && !a.RequestPath.StartsWith("/Api")
        ).ToAggregateAsync(g => new
        {
            Count = g.Count()
        });
    }
}