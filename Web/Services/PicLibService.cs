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

    [Obsolete("Some images may break proportions or fail cropping")]
    public static async Task<(Image, IImageFormat)> GenerateSizedImageAsyncOld(string imagePath, int width, int height)
    {
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        var (image, format) = await Image.LoadWithFormatAsync(fileStream);

        var originWidth = image.Width;
        var originHeight = image.Height;

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
                // Converting to int after scaling loses decimal places, so add 1
                newWidth = image.Width / image.Height * newHeight + 1;
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
                newHeight = newWidth * image.Height / image.Width + 1;
            }

            cropRect = new Rectangle(0, (newHeight - height) / 2, width, height);
        }

        image.Mutate(a => a.Resize(newWidth, newHeight));

        try
        {
            image.Mutate(a => a.Crop(cropRect));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"crop image error: {ex.Message}");
            Console.WriteLine($"image size={originWidth}, {originHeight}; " +
                              $"new size={newWidth},{newHeight}; " +
                              $"crop size={width},{height}");
        }

        return (image, format);
    }

    /// <summary>
    ///     Calculate Greatest Common Divisor and Least Common Multiple
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns>Greatest Common Divisor, Least Common Multiple</returns>
    private static (int, int) GetGreatestCommonDivisor(int num1, int num2)
    {
        // Define variable to save product of two numbers
        var product = num1 * num2;

        // Define temporary variable to save remainder
        var temp = num1 % num2;
        // Euclidean algorithm
        do
        {
            num1 = num2;
            num2 = temp;
            temp = num1 % num2;
        } while (temp != 0);

        // Greatest Common Divisor, Least Common Multiple
        return (num2, product / num2);
    }

    /// <summary>
    ///     Get Image Aspect Ratio
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns>Width x Height</returns>
    private static (double, double) GetPhotoScale(int width, int height)
    {
        var (gcd, _) = GetGreatestCommonDivisor(width, height);
        return ((double)width / gcd, (double)height / gcd);
    }

    /// <summary>
    ///     Generate Sized Image
    /// </summary>
    /// <param name="imagePath"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private async Task<(Image, IImageFormat)> GenerateSizedImageAsync(string imagePath, int width, int height)
    {
        await using var fileStream = new FileStream(imagePath, FileMode.Open);
        var (image, format) = await Image.LoadWithFormatAsync(fileStream);

        // Output size exceeds original image size, enlarge
        if (width > image.Width && height > image.Height)
        {
            image.Mutate(a => a.Resize(width, height));
        }
        else if (width > image.Width || height > image.Height)
        {
            // Change the side with larger ratio
            if (width / image.Width < height / image.Height)
                image.Mutate(a => a.Resize(0, height));
            else
                image.Mutate(a => a.Resize(width, 0));
        }

        // Use input size as cropping ratio
        var (scaleWidth, scaleHeight) = GetPhotoScale(width, height);
        var cropWidth = image.Width;
        var cropHeight = (int)(image.Width / scaleWidth * scaleHeight);
        if (cropHeight > image.Height)
        {
            cropHeight = image.Height;
            cropWidth = (int)(image.Height / scaleHeight * scaleWidth);
        }

        var cropRect = new Rectangle((image.Width - cropWidth) / 2, (image.Height - cropHeight) / 2, cropWidth,
            cropHeight);
        image.Mutate(a => a.Crop(cropRect));
        image.Mutate(a => a.Resize(width, height));

        return (image, format);
    }

    /// <summary>
    ///     Get Random Image from Picture Library
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
    ///     Get Random Image from Picture Library
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