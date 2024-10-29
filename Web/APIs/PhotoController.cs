using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;
using Web.ViewModels.Response;
using Web.Extensions;

namespace Web.Apis;

/// <summary>
/// Photography
/// </summary>
[ApiController]
[Route("Api/[controller]")]
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
        if (photo == null) return ApiResponse.NotFound(Response);
        return new ApiResponse<Photo> { Data = photo };
    }

    [HttpPost]
    public ApiResponse<Photo> Add(Photo photo)
    {
        return new ApiResponse<Photo> { Data = photo };
    }

    [HttpDelete("{id}")]
    public IApiResponse Delete(string id)
    {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound(Response);
        var rows = _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok(Response, $"deleted {rows} rows.")
            : ApiResponse.BadRequest(Response, "deleting failed.");
    }
}
