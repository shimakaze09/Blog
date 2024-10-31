using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Web.Apis;

[ApiController]
[Route("api/[controller]/[action]")]
public class DataWrapperTestController : ControllerBase
{
    private readonly IFileProvider _fileProvider;

    // This controller is used to test DataWrapper functionality
    public DataWrapperTestController(IWebHostEnvironment env)
    {
        _fileProvider = env.WebRootFileProvider;
    }

    [HttpGet]
    public IActionResult NoFoundResult()
        => NotFound();

    [HttpGet]
    public IActionResult UnAuthorizedResult()
        => Unauthorized();

    [HttpGet]
    public IActionResult ProblemDetailResult()
        => Problem("There has a problemDetail.If MiCake WrapProblemDetails is true,will be wrapped");

    [HttpGet]
    public IActionResult FileResult()
    {
        FileStreamResult result = null;

        var logoFile = _fileProvider.GetFileInfo("images/codelab.jpg");
        if (!logoFile.Exists)
            return NotFound("File doesn't exists");

        result = new FileStreamResult(logoFile.CreateReadStream(), "image/png");
        return result;
    }

    [HttpGet]
    public void VoidResult()
    {
    }

    [HttpGet]
    public Task VoidAsyncResult()
    {
        return Task.CompletedTask;
    }

    [HttpGet]
    public string StringResult()
    {
        return "There result will be wrapped by micake.";
    }

    [HttpGet]
    public int IntResult()
    {
        return 1008611;
    }

    [HttpGet]
    public List<int> ListResult()
    {
        return new List<int>() { 1, 2, 3, 4, 5 };
    }

    [HttpGet]
    public IActionResult SoftlyExceptionResult()
    {
        throw new Exception("The exception will be wrapped");
    }

    [HttpGet]
    public IActionResult NormalExceptionResult()
    {
        throw new Exception("The exception will be wrapped as a custom exception type. When IsDebug is set to true, stack trace information will be displayed.");
    }
}
