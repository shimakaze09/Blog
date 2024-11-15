using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Blog;

namespace Web.APIs.Blog;

/// <summary>
///     Blog
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class BlogController : ControllerBase
{
    private readonly BlogService _blogService;

    public BlogController(BlogService blogService)
    {
        _blogService = blogService;
    }

    /// <summary>
    ///     Get top blog post
    /// </summary>
    /// <returns></returns>
    [HttpGet("top")]
    public ApiResponse<Post> GetTopOnePost()
    {
        return new ApiResponse<Post> { Data = _blogService.GetTopOnePost() };
    }

    /// <summary>
    ///     Get recommended blog posts, with a maximum of two posts per row
    /// </summary>
    /// <returns></returns>
    [HttpGet("featured")]
    public ApiResponse<List<Post>> GetFeaturedPosts()
    {
        return new ApiResponse<List<Post>>(_blogService.GetFeaturedPosts());
    }

    /// <summary>
    ///     Blog overview information
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse<BlogOverview> Overview()
    {
        return new ApiResponse<BlogOverview>(_blogService.Overview());
    }

    /// <summary>
    ///     Blog article status list
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse GetStatusList()
    {
        return ApiResponse.Ok(_blogService.GetStatusList());
    }

    /// <summary>
    ///     Uploads a blog post compressed file and imports it.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the uploaded post.</returns>
    [HttpPost("[action]")]
    public async Task<ApiResponse<Post>> Upload([FromForm] PostCreationDto dto, IFormFile file,
        [FromServices] CategoryService categoryService)
    {
        if (!file.FileName.EndsWith(".zip")) return ApiResponse.BadRequest("Only zip files are allowed.");

        var category = categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"Category {dto.CategoryId} does not exist!");
        try
        {
            return new ApiResponse<Post>(await _blogService.Upload(dto, file));
        }
        catch (Exception ex)
        {
            return ApiResponse.Error($"Error extracting file: {ex.Message}");
        }
    }
}