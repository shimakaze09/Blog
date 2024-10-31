using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;
using Web.ViewModels;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
/// Blog
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly BlogService _blogService;

    public BlogController(BlogService blogService)
    {
        _blogService = blogService;
    }

    /// <summary>
    /// Get top blog post
    /// </summary>
    /// <returns></returns>
    [HttpGet("top")]
    public ApiResponse<Post> GetTopOnePost()
    {
        return new ApiResponse<Post> { Data = _blogService.GetTopOnePost() };
    }

    /// <summary>
    /// Get recommended blog posts, with a maximum of two posts per row
    /// </summary>
    /// <returns></returns>
    [HttpGet("featured")]
    public ApiResponse<List<List<Post>>> GetFeaturedPostRows()
    {
        return new ApiResponse<List<List<Post>>> { Data = _blogService.GetFeaturedPostRows() };
    }
}
