using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Blog;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
///     Blog
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
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
    public ApiResponse<List<Post>> GetFeaturedPostRows()
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
    /// Blog article status list
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse GetStatusList()
    {
        return ApiResponse.Ok(_blogService.GetStatusList());
    }
}