using Data.Models;
using FreeSql;
using Markdig;
using Web.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace Web.Services;

public class PostService
{
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IWebHostEnvironment _environment;

    public PostService(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo, IWebHostEnvironment environment)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
    }

    public Post? GetById(string id)
    {
        return _postRepo.Where(a => a.Id == id).Include(a => a.Category).First();
    }

    public int Delete(string id)
    {
        return _postRepo.Delete(a => a.Id == id);
    }

    public Post InsertOrUpdate(Post post)
    {
        return _postRepo.InsertOrUpdate(post);
    }

    /// <summary>
    /// Uploads an image for a specific article
    /// </summary>
    /// <param name="post">The post object</param>
    /// <param name="file">The uploaded file</param>
    /// <returns>The URL of the saved image</returns>
    public string UploadImage(Post post, IFormFile file)
    {
        InitPostMediaDir(post);

        var fileRelativePath = Path.Combine("media", "blog", post.Id, file.FileName);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        using (var fs = new FileStream(savePath, FileMode.Create))
        {
            file.CopyTo(fs);
        }

        return Path.Combine("http://127.0.0.1:5205", fileRelativePath);
    }

    /// <summary>
    /// Gets the image resources for a specified article
    /// </summary>
    /// <param name="post">The article object</param>
    /// <returns>A list of image URLs</returns>
    public List<string> GetImages(Post post)
    {
        var data = new List<string>();
        var postDir = InitPostMediaDir(post);
        foreach (var file in Directory.GetFiles(postDir))
        {
            data.Add(Path.Combine(
                "http://127.0.0.1:5205", "media", "blog", post.Id, file
            ));
        }
        return data;
    }

    public IPagedList<Post> GetPagedList(int categoryId = 0, int page = 1, int pageSize = 10)
    {
        List<Post> posts;
        if (categoryId == 0)
            posts = _postRepo.Select
                .OrderByDescending(a => a.LastModifiedTime)
                .Include(a => a.Category)
                .ToList();
        else
            posts = _postRepo.Where(a => a.CategoryId == categoryId)
                .OrderByDescending(a => a.LastModifiedTime)
                .Include(a => a.Category)
                .ToList();

        return posts.ToPagedList(page, pageSize);
    }

    /// <summary>
    ///     Converts a Post object to a PostViewModel object
    /// </summary>
    /// <param name="post">The Post object to convert</param>
    /// <returns>The converted PostViewModel object</returns>
    public PostViewModel GetPostViewModel(Post post)
    {
        var vm = new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary,
            Content = post.Content,
            ContentHtml = Markdig.Markdown.ToHtml(post.Content),
            Path = post.Path,
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastModifiedTime,
            Category = post.Category,
            Categories = new List<Category>()
        };


        foreach (var itemId in post.Categories.Split(",").Select(int.Parse))
        {
            var item = _categoryRepo.Where(a => a.Id == itemId).First();
            if (item != null) vm.Categories.Add(item);
        }

        return vm;
    }

    private string InitPostMediaDir(Post post)
    {
        var blogMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id);
        if (!Directory.Exists(blogMediaDir)) Directory.CreateDirectory(blogMediaDir);
        if (!Directory.Exists(postMediaDir)) Directory.CreateDirectory(postMediaDir);

        return postMediaDir;
    }
}