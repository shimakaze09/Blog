using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ILogger<BlogController> _logger;

    public BlogController(BlogService blogService, ILogger<BlogController> logger)
    {
        _blogService = blogService;
        _logger = logger;
    }

    /// <summary>
    ///     Get top blog post
    /// </summary>
    /// <returns></returns>
    [HttpGet("top")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Post?>))]
    public async Task<Post?> GetTopOnePost()
    {
        return await _blogService.GetTopOnePost();
    }

    /// <summary>
    ///     Get recommended blog posts, with a maximum of two posts per row
    /// </summary>
    /// <returns></returns>
    [HttpGet("featured")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<Post>>))]
    public async Task<List<Post>> GetFeaturedPosts()
    {
        return await _blogService.GetFeaturedPosts();
    }

    /// <summary>
    ///     Blog overview information
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BlogOverview>))]
    public async Task<BlogOverview> Overview()
    {
        return await _blogService.Overview();
    }

    /// <summary>
    ///     Blog article status list
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string?>))]
    public async Task<List<string?>> GetStatusList()
    {
        return await _blogService.GetStatusList();
    }

    /// <summary>
    ///     Uploads a blog post compressed file and imports it.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the uploaded post.</returns>
    [Authorize]
    [HttpPost("[action]")]
    public async Task<ApiResponse<Post>> Upload([FromForm] PostCreationDto dto, IFormFile file,
        [FromServices] CategoryService categoryService)
    {
        if (!file.FileName.EndsWith(".zip")) return ApiResponse.BadRequest("Only zip files are allowed.");

        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"Category {dto.CategoryId} does not exist!");
        try
        {
            return new ApiResponse<Post>(await _blogService.Upload(dto, file));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Decompression error: {message}", ex.Message);
            return ApiResponse.Error($"Error extracting file: {ex.Message}");
        }
    }
}