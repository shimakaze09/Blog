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

    public VisitRecord? GetById(int id)
    {
        var item = _repo.Where(a => a.Id == id).First();
        return item;
    }

    public List<VisitRecord> GetAll()
    {
        return _repo.Select.OrderByDescending(a => a.Time).ToList();
    }

    public IPagedList<VisitRecord> GetPagedList(VisitRecordQueryParameters param)
    {
        var querySet = _repo.Select;

        // Search
        if (!string.IsNullOrEmpty(param.Search)) querySet = querySet.Where(a => a.RequestPath.Contains(param.Search));

        // Sort
        if (!string.IsNullOrEmpty(param.SortBy))
        {
            // Determine if ascending order
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');

            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        return querySet.ToList().ToPagedList(param.Page, param.PageSize);
    }

    /// <summary>
    ///     Summary data
    /// </summary>
    /// <returns></returns>
    public object Overview()
    {
        var querySet = _repo.Where(a => !a.RequestPath.StartsWith("/Api"));
        return new
        {
            TotalVisit = querySet.Count(),
            TodayVisit = querySet.Where(a => a.Time.Date == DateTime.Today).Count()
        };
    }

    /// <summary>
    ///     Statistics data
    /// </summary>
    /// <returns></returns>
    public object Stats(DateTime date)
    {
        var data = _repo.Where(
            a => a.Time.Date == date.Date
                 && !a.RequestPath.StartsWith("/Api")
        );
        return new
        {
            Count = data.Count()
        };
    }
}