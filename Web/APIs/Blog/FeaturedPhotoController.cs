using CodeLab.Share.ViewModels.Response;
using Data.Models;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;

namespace Web.Apis.Blog;

/// <summary>
///     Featured Photo
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
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
    public async Task<List<FeaturedPhoto>> GetList()
    {
        return await _fpRepo.Select
            .Include(a => a.Photo)
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ApiResponse<FeaturedPhoto>> Get(int id)
    {
        var item = await _fpRepo.Where(a => a.Id == id).FirstAsync();
        return item == null
            ? ApiResponse.NotFound($"Featured photo record {id} does not exist")
            : new ApiResponse<FeaturedPhoto>(item);
    }

    [HttpPost]
    public async Task<ApiResponse<FeaturedPhoto>> Add(string photoId)
    {
        var photo = await _photoService.GetById(photoId);
        return photo == null
            ? ApiResponse.NotFound($"Photo {photoId} does not exist")
            : new ApiResponse<FeaturedPhoto>(await _photoService.AddFeaturedPhoto(photo));
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id)
    {
        var item = await _fpRepo.Where(a => a.Id == id).FirstAsync();
        if (item == null) return ApiResponse.NotFound($"Featured photo record {id} does not exist");
        var rows = await _fpRepo.DeleteAsync(item);
        return ApiResponse.Ok($"Deleted {rows} rows.");
    }
}