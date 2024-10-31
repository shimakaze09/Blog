using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Photography;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
/// Photography
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PhotoController : ControllerBase
{
    private readonly PhotoService _photoService;

    public PhotoController(PhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpGet]
    public ApiResponsePaged<Photo> GetList(int page = 1, int pageSize = 10)
    {
        var paged = _photoService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Photo>
        {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [HttpGet("{id}")]
    public ApiResponse<Photo> Get(string id)
    {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound();
        return new ApiResponse<Photo> { Data = photo };
    }

    [Authorize]
    [HttpPost]
    public ApiResponse<Photo> Add([FromForm] PhotoCreationDto dto, IFormFile file)
    {
        var photo = _photoService.Add(dto, file);

        return !ModelState.IsValid
            ? ApiResponse.BadRequest(ModelState)
            : new ApiResponse<Photo>(photo);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public ApiResponse Delete(string id)
    {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound();
        var rows = _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error(Response, "deleting failed.");
    }

    /// <summary>
    /// Rebuild image library data (rescan each image's size etc.)
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    public ApiResponse ReBuildData()
    {
        return ApiResponse.Ok(new
        {
            Rows = _photoService.ReBuildData()
        }, "Rebuild image library data");
    }

    /// <summary>
    /// Batch import images
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    public ApiResponse<List<Photo>> BatchImport()
    {
        var result = _photoService.BatchImport();
        return new ApiResponse<List<Photo>>
        {
            Data = result,
            Message = $"Successfully imported {result.Count} photos"
        };
    }
}
