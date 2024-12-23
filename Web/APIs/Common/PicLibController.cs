using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using Web.Extensions;
using Web.Services;

namespace Web.APIs.Common;

/// <summary>
///     Image Library
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public class PicLibController : ControllerBase
{
    private readonly PicLibService _service;

    public PicLibController(PicLibService service)
    {
        _service = service;
    }

    private static async Task<IActionResult> GenerateImageResponse(Image image, IImageFormat format)
    {
        var encoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(format);
        await using var stream = new MemoryStream();
        await image.SaveAsync(stream, encoder);
        return new FileContentResult(stream.GetBuffer(), "image/jpeg");
    }

    /// <summary>
    ///     Get a random image with specified dimensions
    /// </summary>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    /// <returns></returns>
    [HttpGet("Random/{width:int}/{height:int}")]
    public async Task<IActionResult> GetRandomImage(int width, int height)
    {
        var (image, format) = await _service.GetRandomImageAsync(width, height);
        return await GenerateImageResponse(image, format);
    }

    /// <summary>
    ///     Get a random image with specified dimensions and seed
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    [HttpGet("Random/{seed}/{width:int}/{height:int}")]
    public async Task<IActionResult> GetRandomImage(string seed, int width, int height)
    {
        var (image, format) = await _service.GetRandomImageAsync(width, height, seed);
        return await GenerateImageResponse(image, format);
    }

    /// <summary>
    ///     Get a random square image with specified side length
    /// </summary>
    /// <param name="sideLength">Side Length</param>
    /// <returns></returns>
    [HttpGet("Random/{sideLength:int}")]
    public async Task<IActionResult> GetRandomImage(int sideLength)
    {
        var (image, format) = await _service.GetRandomImageAsync(sideLength, sideLength);
        return await GenerateImageResponse(image, format);
    }

    /// <summary>
    ///     Get a random square image with specified side length and seed
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="sideLength">Side Length</param>
    /// <returns></returns>
    [HttpGet("Random/{seed}/{sideLength:int}")]
    public async Task<IActionResult> GetRandomImage(string seed, int sideLength)
    {
        var (image, format) = await _service.GetRandomImageAsync(sideLength, sideLength, seed);
        return await GenerateImageResponse(image, format);
    }

    /// <summary>
    ///     Get a random image
    /// </summary>
    /// <returns></returns>
    [HttpGet("Random")]
    public async Task<IActionResult> GetRandomImage()
    {
        var (image, format) = await _service.GetRandomImageAsync();
        return await GenerateImageResponse(image, format);
    }

    /// <summary>
    ///     Get a random image with initial seed
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    [HttpGet("Random/{seed}")]
    public async Task<IActionResult> GetRandomImage(string seed)
    {
        var (image, format) = await _service.GetRandomImageAsync(seed);
        return await GenerateImageResponse(image, format);
    }
}