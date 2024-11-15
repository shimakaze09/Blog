using CodeLab.Share.ViewModels.Response;
using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;

namespace Web.APIs.Blog;

/// <summary>
///     Featured Posts
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class FeaturedPostController : ControllerBase
{
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;
    private readonly IBaseRepository<Post> _postRepo;

    public FeaturedPostController(IBaseRepository<FeaturedPost> featuredPostRepo, IBaseRepository<Post> postRepo)
    {
        _featuredPostRepo = featuredPostRepo;
        _postRepo = postRepo;
    }

    [HttpGet]
    public ApiResponse<List<FeaturedPost>> GetList()
    {
        return new ApiResponse<List<FeaturedPost>>(
            _featuredPostRepo.Select.Include(a => a.Post.Category).ToList()
        );
    }

    [HttpGet("{id:int}")]
    public ApiResponse<FeaturedPost> Get(int id)
    {
        var item = _featuredPostRepo.Where(a => a.Id == id)
            .Include(a => a.Post).First();
        return item == null ? ApiResponse.NotFound() : new ApiResponse<FeaturedPost>(item);
    }

    [HttpPost]
    public ApiResponse<FeaturedPost> Add([FromQuery] string postId)
    {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound($"Blog post {postId} does not exist");
        var item = _featuredPostRepo.Insert(new FeaturedPost { PostId = postId });
        return new ApiResponse<FeaturedPost>(item);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id)
    {
        var item = _featuredPostRepo.Where(a => a.Id == id).First();
        if (item == null)
            return ApiResponse.NotFound($"Recommended blog posts {id} do not exist");
        var rows = _featuredPostRepo.Delete(item);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}