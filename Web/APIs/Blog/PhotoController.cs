using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Photography;

namespace Web.APIs.Blog;

/// <summary>
///     Photography
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
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
        return photo == null
            ? ApiResponse.NotFound($"Photo {id} does not exist")
            : new ApiResponse<Photo> { Data = photo };
    }

    [HttpGet("{id}/Thumb")]
    public async Task<IActionResult> GetThumb(string id, [FromQuery] int width = 300)
    {
        var data = await _photoService.GetThumb(id, width);
        return new FileContentResult(data, "image/jpeg");
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
        if (photo == null) return ApiResponse.NotFound($"Photo {id} does not exist");
        var rows = _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error("deleting failed.");
    }

    /// <summary>
    ///     Set as featured image
    /// </summary>
    /// <param name="id">The ID of the photo</param>
    /// <returns>A response containing the featured photo</returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse<FeaturedPhoto> SetFeatured(string id)
    {
        var photo = _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"Image {id} does not exist")
            : new ApiResponse<FeaturedPhoto>(_photoService.AddFeaturedPhoto(photo));
    }

    /// <summary>
    ///     Remove from featured
    /// </summary>
    /// <param name="id">The ID of the photo</param>
    /// <returns>A response indicating success or failure</returns>
    [HttpPost("{id}/[action]")]
    public ApiResponse CancelFeatured(string id)
    {
        var photo = _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Image {id} does not exist");
        var rows = _photoService.DeleteFeaturedPhoto(photo);
        return ApiResponse.Ok($"Deleted {rows} rows.");
    }

    /// <summary>
    ///     Rebuild image library data (rescan each image's size etc.)
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
    ///     Batch import images
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    [ApiExplorerSettings(GroupName = "blog")]
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