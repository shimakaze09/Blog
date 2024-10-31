using Microsoft.AspNetCore.Mvc;
using Data.Models;
using Web.Services;
using Web.ViewModels.Response;

namespace Web.Apis;

/// <summary>
/// Featured Photos
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeaturedPhotoController : ControllerBase
{
    private readonly PhotoService _photoService;

    public FeaturedPhotoController(PhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpGet]
    public ApiResponse<List<Photo>> GetList()
    {
        return new ApiResponse<List<Photo>>(_photoService.GetFeaturedPhotos());
    }
}
