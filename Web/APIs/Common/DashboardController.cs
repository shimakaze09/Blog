using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;

namespace Web.APIs.Common;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public class DashboardController : ControllerBase
{
    [HttpGet("[action]")]
    public ApiResponse ClrStats()
    {
        return ApiResponse.Ok();
    }
}