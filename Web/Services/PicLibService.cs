using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Web.Services;

/// <summary>
///     Picture Library Service
/// </summary>
public class PicLibService
{
    private readonly IWebHostEnvironment _environment;
    private readonly Random _random;

    public PicLibService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _random = Random.Shared;
        var importPath = Path.Combine(_environment.WebRootPath, "media", "picture_library");
        var root = new DirectoryInfo(importPath);
        foreach (var file in root.GetFiles()) ImageList.Add(file.FullName);
    }

    public List<string> ImageList { get; set; } = new();

    /// <summary>
    ///     Generate image of specified size
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static async Task<(Image, IImageFormat)> GenerateSizedImageAsync(string imagePath, int width, int height)
    {
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        var (image, format) = await Image.LoadWithFormatAsync(fileStream);
        Rectangle cropRect;
        int newWidth;
        int newHeight;
        // Landscape image
        if (image.Width > image.Height)
        {
            if (width > image.Width)
            {
                newWidth = width;
                newHeight = height;
            }
            else
            {
                newHeight = height;
                newWidth = image.Width / image.Height * newHeight;
            }

            cropRect = new Rectangle((newWidth - width) / 2, 0, width, height);
        }
        // Portrait image
        else
        {
            if (height > image.Height)
            {
                newWidth = width;
                newHeight = height;
            }
            else
            {
                newWidth = width;
                newHeight = newWidth * image.Height / image.Width;
            }

            cropRect = new Rectangle(0, (newHeight - height) / 2, width, height);
        }

        image.Mutate(a => a.Resize(newWidth, newHeight));
        image.Mutate(a => a.Crop(cropRect));
        return (image, format);
    }

    /// <summary>
    ///     Get random image from the library
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    public async Task<(Image, IImageFormat)> GetRandomImageAsync(int width, int height, string? seed = null)
    {
        var rnd = seed == null ? _random : new Random(seed.GetHashCode());
        var imagePath = ImageList[rnd.Next(0, ImageList.Count)];
        return await GenerateSizedImageAsync(imagePath, width, height);
    }

    /// <summary>
    ///     Get random image from the library
    /// </summary>
    /// <param name="seed"></param>
    /// <returns></returns>
    public async Task<(Image, IImageFormat)> GetRandomImageAsync(string? seed = null)
    {
        var rnd = seed == null ? _random : new Random(seed.GetHashCode());
        var imagePath = ImageList[rnd.Next(0, ImageList.Count)];
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        return await Image.LoadWithFormatAsync(fileStream);
    }
}