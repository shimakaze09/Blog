using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Categories;

namespace Web.APIs.Blog;

/// <summary>
///     Featured Categories
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class FeaturedCategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public FeaturedCategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponse<List<FeaturedCategory>> GetAll()
    {
        return new ApiResponse<List<FeaturedCategory>>(_categoryService.GetFeaturedCategories());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ApiResponse<FeaturedCategory> Get(int id)
    {
        var item = _categoryService.GetFeaturedCategoryById(id);
        return item == null
            ? ApiResponse.NotFound($"Recommended category record {id} does not exist")
            : new ApiResponse<FeaturedCategory>(item);
    }

    [HttpPost]
    public ApiResponse<FeaturedCategory> Add(FeaturedCategoryCreationDto2 dto2)
    {
        var category = _categoryService.GetById(dto2.CategoryId);
        if (category == null) return ApiResponse.NotFound($"Category {dto2.CategoryId} does not exist");
        var item = _categoryService.AddOrUpdateFeaturedCategory(category, new FeaturedCategoryCreationDto
        {
            Name = dto2.Name,
            Description = dto2.Description,
            IconCssClass = dto2.IconCssClass
        });
        return new ApiResponse<FeaturedCategory>(item);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id)
    {
        var item = _categoryService.GetFeaturedCategoryById(id);
        if (item == null) return ApiResponse.NotFound($"Recommended category record {id} does not exist");
        var rows = _categoryService.DeleteFeaturedCategoryById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}