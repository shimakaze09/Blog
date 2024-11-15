using AutoMapper;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.Links;
using Web.ViewModels.Response;

namespace Web.Apis.Links;

/// <summary>
///     Friend links
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = "blog")]
public class LinkController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly LinkService _service;

    public LinkController(LinkService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
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

    [HttpPost]
    public ApiResponse<Link> Add(LinkCreationDto dto)
    {
        var link = _mapper.Map<Link>(dto);
        link = _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpPut("{id:int}")]
    public ApiResponse<Link> Update(int id, LinkCreationDto dto)
    {
        var item = _service.GetById(id);
        if (item == null) return ApiResponse.NotFound();

        var link = _mapper.Map(dto, item);
        link = _service.AddOrUpdate(link);
        return new ApiResponse<Link>(link);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id)
    {
        if (!_service.HasId(id)) return ApiResponse.NotFound();
        var rows = _service.DeleteById(id);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}