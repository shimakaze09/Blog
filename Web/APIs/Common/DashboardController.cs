using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.Response;

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