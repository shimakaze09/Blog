using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
namespace Web.Apis;

[ApiController]
[Route("Api/[controller]")]
public class BlogPostController : ControllerBase
{
    private readonly IBaseRepository<Post> _postRepo;

    public BlogPostController(IBaseRepository<Post> postRepo)
    {
        _postRepo = postRepo;
    }

    [HttpGet]
    public ActionResult<List<Post>> GetAll()
    {
        return _postRepo.Select.Include(a => a.Category).ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<Post> Get(string id)
    {
        var post = _postRepo.Where(a => a.Id == id).First();
        if (post == null) return NotFound();
        return post;
    }
}