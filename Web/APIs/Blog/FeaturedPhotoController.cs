using CodeLab.Share.ViewModels.Response;
using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;

namespace Web.APIs.Blog;

/// <summary>
///     Featured Photos
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class FeaturedPhotoController : ControllerBase
{
    private readonly IBaseRepository<FeaturedPhoto> _fpRepo;
    private readonly PhotoService _photoService;

    public FeaturedPhotoController(PhotoService photoService, IBaseRepository<FeaturedPhoto> fpRepo)
    {
        _photoService = photoService;
        _fpRepo = fpRepo;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponse<List<FeaturedPhoto>> GetList()
    {
        return new ApiResponse<List<FeaturedPhoto>>(_fpRepo.Select
            .Include(a => a.Photo).ToList());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ApiResponse<FeaturedPhoto> Get(int id)
    {
        var item = _fpRepo.Where(a => a.Id == id).First();
        return item == null
            ? ApiResponse.NotFound($"Recommended Photo record {id} does not exist")
            : new ApiResponse<FeaturedPhoto>(item);
    }

    [HttpPost]
    public ApiResponse<FeaturedPhoto> Add(string photoId)
    {
        var photo = _photoService.GetById(photoId);
        return photo == null
            ? ApiResponse.NotFound($"Photo {photoId} does not exist")
            : new ApiResponse<FeaturedPhoto>(_photoService.AddFeaturedPhoto(photo));
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id)
    {
        var item = _fpRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound($"Recommended Photo record {id} does not exist");
        var rows = _fpRepo.Delete(item);
        return ApiResponse.Ok($"Deleted {rows} rows.");
    }
}