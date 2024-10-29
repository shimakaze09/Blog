using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Web.Dtos;
using Web.ViewModels;
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
    public ApiResponse<PostListDto> Get()
    {
        var postId = _topPostRepo.Select.Include(a => a.Post).First(a => a.Post.Id);
        var dto = _postRepo.Where(a => a.Id == postId).First<PostListDto>();
        return new ApiResponse<PostListDto> { Data = dto };
    }

    [HttpPut("[action]")]
    public ApiResponse Set([FromQuery] string postId)
    {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new ApiResponse { Successful = false, Message = "post not found" };
        }
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        _topPostRepo.Insert(new TopPost { PostId = post.Id });
        return new ApiResponse { Successful = true, Message = $"ok. deleted {rows} old topPosts." };
    }

    [HttpDelete]
    public ApiResponse Clear()
    {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        return new ApiResponse { Successful = true, Message = $"deleted {rows} topPosts." };
    }
}