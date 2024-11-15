using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Services;

namespace Web.Apis.Common;

/// <summary>
///     Integration of some DataAcq interfaces
/// </summary>
[ApiController]
[Route("Api/[controller]/[action]")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public class DataAcqController : ControllerBase
{
    private readonly CrawlService _crawlService;

    public DataAcqController(CrawlService crawlService)
    {
        _crawlService = crawlService;
    }

    [HttpGet]
    public async Task<string> Poem()
    {
        return await _crawlService.GetPoem();
    }

    [HttpGet]
    public async Task<string> Hitokoto()
    {
        return await _crawlService.GetHitokoto();
    }
}