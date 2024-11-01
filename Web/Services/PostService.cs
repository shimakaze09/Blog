using Contrib.Utils;
using Data.Models;
using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Web.ViewModels;
using X.PagedList;
using X.PagedList.Extensions;

namespace Web.Services;

public class PostService
{
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IBaseRepository<Post> _postRepo;

    public PostService(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
        _configuration = configuration;
    }

    public string Host => _configuration.GetSection("Server:Host").Value;

    public Post? GetById(string id)
    {
        // When fetching articles, parse markdown image URLs, add full URLs and return them to the frontend
        var post = _postRepo.Where(a => a.Id == id).Include(a => a.Category).First();
        post.Content = MdImageLinkConvert(post);
        return post;
    }

    public int Delete(string id)
    {
        return _postRepo.Delete(a => a.Id == id);
    }

    public Post InsertOrUpdate(Post post)
    {
        // When editing articles, replace the image URLs in markdown with relative paths and save
        post.Content = MdImageLinkConvert(post, false);
        return _postRepo.InsertOrUpdate(post);
    }

    /// <summary>
    ///     Uploads an image for a specific article
    /// </summary>
    /// <param name="post">The post object</param>
    /// <param name="file">The uploaded file</param>
    /// <returns>The URL of the saved image</returns>
    public string UploadImage(Post post, IFormFile file)
    {
        InitPostMediaDir(post);

        var fileRelativePath = Path.Combine("media", "blog", post.Id, file.FileName);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        if (File.Exists(savePath))
        {
            // File renaming for uploaded files
            var newFilename =
                $"{Path.GetFileNameWithoutExtension(file.FileName)}-{GuidUtils.GuidTo16String()}.{Path.GetExtension(file.FileName)}";
            fileRelativePath = Path.Combine("media", "blog", post.Id, newFilename);
            savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        }

        using (var fs = new FileStream(savePath, FileMode.Create))
        {
            file.CopyTo(fs);
        }

        return Path.Combine(Host, fileRelativePath);
    }

    /// <summary>
    ///     Gets the image resources for a specified article
    /// </summary>
    /// <param name="post">The article object</param>
    /// <returns>A list of image URLs</returns>
    public List<string> GetImages(Post post)
    {
        var data = new List<string>();
        var postDir = InitPostMediaDir(post);
        foreach (var file in Directory.GetFiles(postDir))
            data.Add(Path.Combine(Host, "media", "blog", post.Id, Path.GetFileName(file)));
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
            ContentHtml = Markdown.ToHtml(post.Content),
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

    /// <summary>
    ///     Initializes the resource directory for blog posts
    /// </summary>
    /// <param name="post">The blog post object</param>
    /// <returns>The path of the post media directory</returns>
    private string InitPostMediaDir(Post post)
    {
        var blogMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id);
        if (!Directory.Exists(blogMediaDir)) Directory.CreateDirectory(blogMediaDir);
        if (!Directory.Exists(postMediaDir)) Directory.CreateDirectory(postMediaDir);

        return postMediaDir;
    }

    /// <summary>
    ///     Converts image links in Markdown
    /// </summary>
    /// <param name="post">The blog post object</param>
    /// <param name="isAddPrefix">Whether to add a prefix to the image URLs</param>
    /// <returns>The converted Markdown content</returns>
    private string MdImageLinkConvert(Post post, bool isAddPrefix = true)
    {
        var document = Markdown.Parse(post.Content);
        foreach (var node in document.AsEnumerable())
        {
            if (node is not ParagraphBlock { Inline: not null } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline)
            {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;
                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;
                if (isAddPrefix && imgUrl.StartsWith("http")) continue;
                if (isAddPrefix)
                {
                    if (imgUrl.StartsWith("http")) continue;
                    // Set full URL
                    linkInline.Url = $"{Host}/media/blog/{post.Id}/{imgUrl}";
                }
                else
                {
                    // Set relative URL
                    linkInline.Url = Path.GetFileName(imgUrl);
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}