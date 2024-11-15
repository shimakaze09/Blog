using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.APIs.Common;

/// <summary>
///     Page themes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "common")]
public class ThemeController : ControllerBase
{
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpGet]
    public List<Theme> GetAll()
    {
        return _themeService.Themes;
    }
}