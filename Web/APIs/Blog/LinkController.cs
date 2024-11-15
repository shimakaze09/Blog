using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Response;

namespace Web.APIs.Blog;

[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class LinkController : ControllerBase
{
    private readonly LinkService _service;

    public LinkController(LinkService service)
    {
        _service = service;
    }

    [HttpGet]
    public List<Link> GetAll()
    {
        return _service.GetAll();
    }

    [HttpGet("{id:int}")]
    public ApiResponse<Link> Get(int id)
    {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Link>(item);
    }
}