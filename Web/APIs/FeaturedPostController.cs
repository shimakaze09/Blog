using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.ViewModels;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
/// Featured Posts
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeaturedPostController : ControllerBase
{
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;

    public FeaturedPostController(IBaseRepository<FeaturedPost> featuredPostRepo, IBaseRepository<Post> postRepo)
    {
        _featuredPostRepo = featuredPostRepo;
        _postRepo = postRepo;
    }

    [HttpGet]
    public ApiResponse<List<Post>> GetList()
    {
        return new ApiResponse<List<Post>>
        {
            Data = _featuredPostRepo.Select.Include(a => a.Post.Category)
                .ToList(a => a.Post)
        };
    }

    [HttpGet("{id:int}")]
    public ApiResponse<Post> Get(int id)
    {
        var item = _featuredPostRepo.Where(a => a.Id == id)
            .Include(a => a.Post).First();
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Post> { Data = item.Post };
    }

    [HttpPost]
    public ApiResponse Add([FromQuery] string postId)
    {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound();
        _featuredPostRepo.Insert(new FeaturedPost { PostId = postId });
        return ApiResponse.Ok();
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id)
    {
        var item = _featuredPostRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound();
        var rows = _featuredPostRepo.Delete(item);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}
