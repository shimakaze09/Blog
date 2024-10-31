using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
///     Featured Categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class FeaturedCategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public FeaturedCategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public ApiResponse<List<FeaturedCategory>> GetList()
    {
        return new ApiResponse<List<FeaturedCategory>>(_categoryService.GetFeaturedCategories());
    }
}