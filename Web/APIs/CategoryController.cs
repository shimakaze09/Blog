using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Extensions;
using Web.ViewModels;
using Web.ViewModels.Response;
using X.PagedList;
using X.PagedList.Extensions;

namespace Web.Apis;

/// <summary>
/// Article Categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private IBaseRepository<Category> _categoryRepo;

    public CategoryController(IBaseRepository<Category> categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    [HttpGet("All")]
    public List<Category> GetAll()
    {
        return _categoryRepo.Select.ToList();
    }

    [HttpGet]
    public ApiResponsePaged<Category> GetList(int page = 1, int pageSize = 10)
    {
        var paged = _categoryRepo.Select.ToList().ToPagedList(page, pageSize);
        return new ApiResponsePaged<Category>
        {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [HttpGet("{id:int}")]
    public IApiResponse Get(int id)
    {
        var item = _categoryRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound();
        return new ApiResponse<Category> { Data = item };
    }

    /// <summary>
    /// Category Word Cloud
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse<List<object>> WordCloud()
    {
        var list = _categoryRepo.Select.IncludeMany(a => a.Posts).ToList();
        var data = new List<object>();
        foreach (var item in list)
        {
            data.Add(new { name = item.Name, value = item.Posts.Count });
        }

        return new ApiResponse<List<object>> { Data = data };
    }
}
