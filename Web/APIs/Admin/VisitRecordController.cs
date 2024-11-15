using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
using Web.ViewModels.QueryFilters;
using Web.ViewModels.Response;

namespace Web.APIs.Admin;

/// <summary>
///     Visit Record
/// </summary>
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

    [HttpGet]
    public ApiResponsePaged<VisitRecord> GetList([FromQuery] VisitRecordQueryParameters param)
    {
        var pagedList = _service.GetPagedList(param);
        return new ApiResponsePaged<VisitRecord>(pagedList);
    }

    [HttpGet("{id:int}")]
    public ApiResponse<VisitRecord> GetById(int id)
    {
        var item = _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<VisitRecord>(item);
    }

    /// <summary>
    ///     Retrieves all visit records
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public ApiResponse<List<VisitRecord>> GetAll()
    {
        return new ApiResponse<List<VisitRecord>>(_service.GetAll());
    }

    /// <summary>
    ///     Gets an overview of the data
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse Overview()
    {
        return ApiResponse.Ok(_service.Overview());
    }

    /// <summary>
    ///     Trend data
    /// </summary>
    /// <param name="days">Number of days to view data, default is 7 days</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse Trend(int days = 7)
    {
        return ApiResponse.Ok(_service.Trend(days));
    }


    /// <summary>
    ///     Statistical API
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public ApiResponse Stats(int year, int month, int day)
    {
        var date = new DateTime(year, month, day);
        return ApiResponse.Ok(_service.Stats(date));
    }
}