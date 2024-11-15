using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.QueryFilters;

namespace Web.APIs.Admin;

/// <summary>
///     Visit Record
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Admin)]
public class VisitRecordController : ControllerBase
{
    private readonly VisitRecordService _service;

    public VisitRecordController(VisitRecordService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ApiResponsePaged<VisitRecord>> GetList([FromQuery] VisitRecordQueryParameters param)
    {
        var pagedList = await _service.GetPagedList(param);
        return new ApiResponsePaged<VisitRecord>(pagedList);
    }

    [HttpGet("{id:int}")]
    public async Task<ApiResponse<VisitRecord>> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<VisitRecord>(item);
    }

    /// <summary>
    ///     Retrieves all visit records
    /// </summary>
    /// <returns></returns>
    [HttpGet("All")]
    public async Task<List<VisitRecord>> GetAll()
    {
        return await _service.GetAll();
    }

    /// <summary>
    ///     Gets an overview of the data
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Overview()
    {
        return ApiResponse.Ok(await _service.Overview());
    }

    /// <summary>
    ///     Trend data
    /// </summary>
    /// <param name="days">Number of days to view data, default is 7 days</param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Trend(int days = 7)
    {
        return ApiResponse.Ok(await _service.Trend(days));
    }


    /// <summary>
    ///     Statistical API
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<ApiResponse> Stats(int year, int month, int day)
    {
        var date = new DateTime(year, month, day);
        return ApiResponse.Ok(await _service.Stats(date));
    }
}