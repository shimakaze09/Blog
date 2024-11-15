using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.APIs.Common;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = "common")]
public class DashboardController : ControllerBase
{
    [HttpGet("[action]")]
    public ApiResponse ClrStats()
    {
        return ApiResponse.Ok();
    }
}