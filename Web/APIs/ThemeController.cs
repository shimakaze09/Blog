using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Apis;

/// <summary>
/// Page themes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ThemeController : ControllerBase
{
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpGet]
    public ActionResult<List<Theme>> GetAll()
    {
        return _themeService.Themes;
    }
}
