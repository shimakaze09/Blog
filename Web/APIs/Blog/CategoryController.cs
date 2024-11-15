using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Categories;

namespace Web.APIs.Blog;

/// <summary>
///     Article Categories
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _cService;

    public CategoryController(CategoryService cService)
    {
        _cService = cService;
    }

    [AllowAnonymous]
    [HttpGet("Nodes")]
    public ApiResponse<List<CategoryNode>> GetNodes()
    {
        return new ApiResponse<List<CategoryNode>>(_cService.GetNodes());
    }

    [AllowAnonymous]
    [HttpGet("All")]
    public ApiResponse<List<Category>> GetAll()
    {
        return new ApiResponse<List<Category>>(_cService.GetAll());
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponsePaged<Category> GetList(int page = 1, int pageSize = 10)
    {
        var paged = _cService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Category>
        {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IApiResponse Get(int id)
    {
        var item = _cService.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Category> { Data = item };
    }

    /// <summary>
    ///     Category Word Cloud
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("[action]")]
    public ApiResponse<List<object>> WordCloud()
    {
        return new ApiResponse<List<object>>(_cService.GetWordCloud());
    }

    /// <summary>
    ///     Set as featured category
    /// </summary>
    /// <seealso href="https://fontawesome.com/search?m=free">FontAwesome icon library</seealso>
    /// <param name="id"></param>
    /// <param name="dto">Featured information <see cref="FeaturedCategoryCreationDto" /></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse<FeaturedCategory> SetFeatured(int id, [FromBody] FeaturedCategoryCreationDto dto)
    {
        var item = _cService.GetById(id);
        return item == null
            ? ApiResponse.NotFound($"Category {id} does not exist")
            : new ApiResponse<FeaturedCategory>(_cService.AddOrUpdateFeaturedCategory(item, dto));
    }

    /// <summary>
    ///     Cancel featured category
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse CancelFeatured(int id)
    {
        var item = _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"Category {id} does not exist");
        var rows = _cService.DeleteFeaturedCategory(item);
        return ApiResponse.Ok($"Deleted {rows} rows.");
    }

    /// <summary>
    ///     Set category visible
    /// </summary>
    /// <param name="id">The ID of the category</param>
    /// <returns>An API response object</returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse SetVisible(int id)
    {
        var item = _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"Category {id} does not exist");
        var rows = _cService.SetVisibility(item, true);
        return ApiResponse.Ok($"Affected {rows} row(s).");
    }

    /// <summary>
    ///     Set category invisible
    /// </summary>
    /// <param name="id">The ID of the category</param>
    /// <returns>An API response object</returns>
    [HttpPost("{id:int}/[action]")]
    public ApiResponse SetInvisible(int id)
    {
        var item = _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"Category {id} does not exist");
        var rows = _cService.SetVisibility(item, false);
        return ApiResponse.Ok($"Affected {rows} row(s).");
    }
}