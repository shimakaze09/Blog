using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;
using Web.ViewModels.Photography;

namespace Web.APIs.Photography;

/// <summary>
///     Photography
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Photo)]
public class PhotoController : ControllerBase
{
    private readonly PhotoService _photoService;

    public PhotoController(PhotoService photoService)
    {
        _photoService = photoService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<Photo>> GetList(int page = 1, int pageSize = 10)
    {
        var paged = await _photoService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Photo>
        {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<Photo>> Get(string id)
    {
        var photo = await _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"Photo {id} does not exist")
            : new ApiResponse<Photo> { Data = photo };
    }

    /// <summary>
    ///     Gets a thumbnail image of the specified width
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/Thumb")]
    public async Task<IActionResult> GetThumb(string id, [FromQuery] int width = 300)
    {
        var data = await _photoService.GetThumb(id, width);
        return new FileContentResult(data, "image/jpeg");
    }

    [HttpPost]
    public async Task<ApiResponse<Photo>> Add([FromForm] PhotoCreationDto dto, IFormFile file)
    {
        var photo = await _photoService.Add(dto, file);

        return !ModelState.IsValid
            ? ApiResponse.BadRequest(ModelState)
            : new ApiResponse<Photo>(photo);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Photo>> Update(string id, [FromBody] PhotoUpdateDto dto)
    {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Photo {id} does not exist");
        dto.Id = id;
        return new ApiResponse<Photo>(await _photoService.Update(dto))
            { Message = "Updated photo information successfully." };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id)
    {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Photo {id} does not exist");
        var rows = await _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error("deleting failed.");
    }

    /// <summary>
    ///     Set as featured photo
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPhoto>> SetFeatured(string id)
    {
        var photo = await _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"Photo {id} does not exist")
            : new ApiResponse<FeaturedPhoto>(await _photoService.AddFeaturedPhoto(photo));
    }

    /// <summary>
    ///     Cancel featured
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id)
    {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Photo {id} does not exist");
        var rows = await _photoService.DeleteFeaturedPhoto(photo);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }

    /// <summary>
    ///     Rebuild photo library data (rescan the size and other data of each photo)
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ApiResponse> ReBuildData()
    {
        return ApiResponse.Ok(new
        {
            Rows = await _photoService.ReBuildData()
        }, "Rebuild photo library data");
    }

    /// <summary>
    ///     Batch import photos
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ApiResponse<List<Photo>>> BatchImport()
    {
        var result = await _photoService.BatchImport();
        return new ApiResponse<List<Photo>>
        {
            Data = result,
            Message = $"Successfully imported {result.Count} photos"
        };
    }
}