using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;

namespace Web.Apis;

/// <summary>
/// Blog
/// </summary>
[ApiController]
[Route("Api/[controller]")]
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
    public ActionResult<Post?> GetTopOnePost()
    {
        return _blogService.GetTopOnePost();
    }
    
    /// <summary>
    /// Get recommended blog posts, with a maximum of two posts per row
    /// </summary>
    /// <returns></returns>
    [HttpGet("featured")]
    public ActionResult<List<List<Post>>> GetFeaturedPosts()
    {
        return _blogService.GetFeaturedPosts();
    }
}
