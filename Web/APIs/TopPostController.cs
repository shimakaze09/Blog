using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Response;

namespace Web.Apis;

[ApiController]
[Route("api/[controller]")]
public class TopPostController : ControllerBase
{
    private readonly IBaseRepository<TopPost> _topPostRepo;
    private readonly IBaseRepository<Post> _postRepo;

    public TopPostController(IBaseRepository<TopPost> topPostRepo, IBaseRepository<Post> postRepo)
    {
        _topPostRepo = topPostRepo;
        _postRepo = postRepo;
    }

    [HttpGet]
    public ApiResponse<Post> Get()
    {
        var data = _topPostRepo.Select.Include(a => a.Post).First(a => a.Post);
        return data == null ? ApiResponse.NoContent(Response) : new ApiResponse<Post>(data: data);
    }

    [HttpPut("[action]")]
    public ApiResponse Set([FromQuery] string postId)
    {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound(Response);

        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        _topPostRepo.Insert(new TopPost { PostId = post.Id });
        return ApiResponse.Ok(Response, $"ok. deleted {rows} old topPosts.");
    }

    [HttpDelete]
    public ApiResponse Clear()
    {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        return ApiResponse.Ok(Response, $"ok. deleted {rows} old topPosts.");
    }
}