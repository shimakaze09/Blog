using AutoMapper;
using Data.Models;
using FreeSql;
using ImageMagick;
using Share.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Web.ViewModels.Photography;
using X.PagedList;

namespace Web.Services;

public class PhotoService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IBaseRepository<FeaturedPhoto> _featuredPhotoRepo;
    private readonly IMapper _mapper;
    private readonly IBaseRepository<Photo> _photoRepo;

    public PhotoService(IBaseRepository<Photo> photoRepo, IWebHostEnvironment environment,
        IBaseRepository<FeaturedPhoto> featuredPhotoRepo, IMapper mapper)
    {
        _photoRepo = photoRepo;
        _environment = environment;
        _featuredPhotoRepo = featuredPhotoRepo;
        _mapper = mapper;
    }

    public async Task<List<Photo>> GetAll()
    {
        return await _photoRepo.Select.ToListAsync();
    }

    public async Task<IPagedList<Photo>> GetPagedList(int page = 1, int pageSize = 10)
    {
        IPagedList<Photo> pagedList = new StaticPagedList<Photo>(
            await _photoRepo.Select.OrderByDescending(a => a.CreateTime).ToListAsync(),
            page, pageSize, Convert.ToInt32(await _photoRepo.Select.CountAsync())
        );
        return pagedList;
    }

    public async Task<List<Photo>> GetFeaturedPhotos()
    {
        return await _featuredPhotoRepo.Select
            .Include(a => a.Photo).ToListAsync(a => a.Photo);
    }

    public async Task<Photo?> GetById(string id)
    {
        return await _photoRepo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<Photo?> GetNext(string id)
    {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return null;
        var next = await _photoRepo
            .Where(a => a.CreateTime < photo.CreateTime && a.Id != id)
            .OrderByDescending(a => a.CreateTime)
            .FirstAsync();
        return next;
    }

    public async Task<Photo?> GetPrevious(string id)
    {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return null;
        var next = await _photoRepo
            .Where(a => a.CreateTime > photo.CreateTime && a.Id != id)
            .OrderBy(a => a.CreateTime)
            .FirstAsync();
        return next;
    }

    /// <summary>
    ///     Generate Progressive JPEG thumbnail (using MagickImage)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="width">Set to 0 to not resize</param>
    public async Task<byte[]> GetThumb(string id, int width = 0)
    {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        using (var image = new MagickImage(GetPhotoFilePath(photo)))
        {
            image.Format = MagickFormat.Pjpeg;
            if (width != 0) image.Resize((uint)width, 0);

            return image.ToByteArray();
        }
    }

    public async Task<Photo> Update(PhotoUpdateDto dto)
    {
        var photo = await GetById(dto.Id);
        photo = _mapper.Map(dto, photo);
        await _photoRepo.UpdateAsync(photo);
        return photo;
    }

    public async Task<Photo> Add(PhotoCreationDto dto, IFormFile photoFile)
    {
        var photoId = GuidUtils.GuidTo16String();
        var photo = new Photo
        {
            Id = photoId,
            Title = dto.Title,
            CreateTime = DateTime.Now,
            Location = dto.Location,
            FilePath = $"{photoId}.jpg"
        };

        var savePath = GetPhotoFilePath(photo);

        // If the image exceeds the size limit, it needs to be resized first
        var resizeFlag = await ResizePhoto(photoFile.OpenReadStream(), savePath);

        // If the size has not been adjusted, save the uploaded image directly
        if (!resizeFlag)
            await using (var fs = new FileStream(savePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(fs);
            }

        photo = await BuildPhotoData(photo);

        return await _photoRepo.InsertAsync(photo);
    }

    /// <summary>
    ///     Get a random photo
    /// </summary>
    public async Task<Photo?> GetRandomPhoto()
    {
        var count = await _photoRepo.Select.CountAsync();
        if (count == 0) return null;

        return await _photoRepo.Select.Take(1).Offset(Random.Shared.Next((int)count)).FirstAsync();
    }

    /// <summary>
    ///     Add a featured photo
    /// </summary>
    public async Task<FeaturedPhoto> AddFeaturedPhoto(Photo photo)
    {
        var item = await _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).FirstAsync();
        if (item != null) return item;
        item = new FeaturedPhoto { PhotoId = photo.Id };
        await _featuredPhotoRepo.InsertAsync(item);
        return item;
    }

    /// <summary>
    ///     Delete a featured photo
    /// </summary>
    public async Task<int> DeleteFeaturedPhoto(Photo photo)
    {
        var item = await _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).FirstAsync();
        return item == null ? 0 : await _featuredPhotoRepo.DeleteAsync(item);
    }

    /// <summary>
    ///     Delete a photo
    ///     <para>Delete the photo file and database record</para>
    /// </summary>
    public async Task<int> DeleteById(string id)
    {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return -1;

        var filePath = GetPhotoFilePath(photo);
        if (File.Exists(filePath)) File.Delete(filePath);
        return await _photoRepo.DeleteAsync(a => a.Id == id);
    }

    /// <summary>
    ///     Rebuild photo library data (rescan the size and other data of each photo)
    /// </summary>
    public async Task<int> ReBuildData()
    {
        var photos = await GetAll();
        var photosUpdate = new List<Photo>();
        foreach (var photo in photos) photosUpdate.Add(await BuildPhotoData(photo));

        return await _photoRepo.UpdateAsync(photosUpdate);
    }

    /// <summary>
    ///     Batch import photos
    /// </summary>
    public async Task<List<Photo>> BatchImport()
    {
        var result = new List<Photo>();
        var importPath = Path.Combine(_environment.WebRootPath, "assets", "photography");
        var root = new DirectoryInfo(importPath);
        foreach (var file in root.GetFiles())
        {
            var photoId = GuidUtils.GuidTo16String();
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            var photo = new Photo
            {
                Id = photoId,
                Title = filename,
                CreateTime = DateTime.Now,
                Location = filename,
                FilePath = $"{photoId}.jpg"
            };
            var savePath = GetPhotoFilePath(photo);

            // If the image exceeds the size limit, it needs to be resized first
            var resizeFlag = await ResizePhoto(new FileStream(file.FullName, FileMode.Open), savePath);

            // If the size has not been adjusted, save the uploaded image directly
            if (!resizeFlag) file.CopyTo(savePath, true);

            photo = await BuildPhotoData(photo);
            await _photoRepo.InsertAsync(photo);
            result.Add(photo);
        }

        return result;
    }

    /// <summary>
    ///     Initialize the photo resource directory
    /// </summary>
    private string InitPhotoMediaDir()
    {
        var dir = Path.Combine(_environment.WebRootPath, "media", "photography");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        return dir;
    }

    /// <summary>
    ///     Get the physical storage path of the photo
    /// </summary>
    private string GetPhotoFilePath(Photo photo)
    {
        return Path.Combine(InitPhotoMediaDir(), photo.FilePath);
    }

    /// <summary>
    ///     Rebuild photo data (scan the size and other data of the photo)
    /// </summary>
    private async Task<Photo> BuildPhotoData(Photo photo)
    {
        var savePath = GetPhotoFilePath(photo);
        var imgInfo = await Image.IdentifyAsync(savePath);
        photo.Width = imgInfo.Width;
        photo.Height = imgInfo.Height;

        return photo;
    }

    /// <summary>
    ///     Resize the image according to the settings
    /// </summary>
    private static async Task<bool> ResizePhoto(Stream stream, string savePath)
    {
        const int maxWidth = 1500;
        const int maxHeight = 1500;
        var resizeFlag = false;

        using var image = await Image.LoadAsync(stream);

        if (image.Width > maxWidth)
        {
            resizeFlag = true;
            image.Mutate(a => a.Resize(maxWidth, 0));
        }

        if (image.Height > maxHeight)
        {
            resizeFlag = true;
            image.Mutate(a => a.Resize(0, maxHeight));
        }

        if (resizeFlag) await image.SaveAsync(savePath);

        return resizeFlag;
    }
}