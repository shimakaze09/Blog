using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;
using Web.ViewModels.Response;

namespace Web.Apis;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "admin")]
public class VisitRecordController : ControllerBase
{
    private readonly VisitRecordService _service;

    public VisitRecordController(VisitRecordService service)
    {
        _service = service;
    }

    [HttpGet("All")]
    public ApiResponse<List<VisitRecord>> GetAll()
    {
        return new ApiResponse<List<VisitRecord>>(_service.GetAll());
    }

    [HttpGet("{id:int}")]
    public ApiResponse<VisitRecord> GetById(int id)
    {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<VisitRecord>(item);
    }
}