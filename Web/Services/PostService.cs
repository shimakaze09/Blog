using System.Net;
using Contrib.Utils;
using Data.Models;
using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Web.ViewModels;
using Web.ViewModels.QueryFilters;
using X.PagedList;
using X.PagedList.Extensions;

namespace Web.Services;

public class PostService
{
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _accessor;
    private readonly LinkGenerator _generator;

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        IHttpContextAccessor accessor,
        LinkGenerator generator
    )
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
        _configuration = configuration;
        _accessor = accessor;
        _generator = generator;
    }

    public string Host => _configuration.GetSection("Server:Host").Value;

    public Post? GetById(string id)
    {
        // When retrieving posts, parse markdown image URLs and add full URLs to return to the frontend
        var post = _postRepo.Where(a => a.Id == id).Include(a => a.Category).First();
        if (post != null) post.Content = MdImageLinkConvert(post, true);
        return post;
    }

    public int Delete(string id)
    {
        return _postRepo.Delete(a => a.Id == id);
    }

    public Post InsertOrUpdate(Post post)
    {
        // When updating posts, replace markdown image URLs with relative paths before saving
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

        var filename = WebUtility.UrlEncode(file.FileName);
        var fileRelativePath = Path.Combine("media", "blog", post.Id, filename);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        if (File.Exists(savePath))
        {
            // Handle file renaming
            var newFilename =
                $"{Path.GetFileNameWithoutExtension(filename)}-{GuidUtils.GuidTo16String()}.{Path.GetExtension(filename)}";
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

    public IPagedList<Post> GetPagedList(PostQueryParameters param)
    {
        var querySet = _postRepo.Select;

        if (param.OnlyPublished)
        {
            querySet = _postRepo.Select.Where(a => a.IsPublished);
        }

        // Is published
        if (param.OnlyPublished)
        {
            querySet = _postRepo.Select.Where(a => a.IsPublished);
        }

        // Status filter
        if (!string.IsNullOrEmpty(param.Status))
        {
            querySet = querySet.Where(a => a.Status == param.Status);
        }

        // Category filtering
        if (param.CategoryId != 0)
        {
            querySet = querySet.Where(a => a.CategoryId == param.CategoryId);
        }

        // Keyword filtering
        if (!string.IsNullOrEmpty(param.Search)) querySet = querySet.Where(a => a.Title.Contains(param.Search));

        // Sorting
        if (!string.IsNullOrEmpty(param.SortBy))
        {
            // Whether ascending order
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');

            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        return querySet.Include(a => a.Category).ToList()
            .ToPagedList(param.Page, param.PageSize);
    }

    /// <summary>
    ///     Convert Post object to PostViewModel object
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
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
            Url = _generator.GetUriByAction(
                _accessor.HttpContext!,
                "Post", "Blog",
                new { Id = post.Id }
            ),
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
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
                    // Set relative link
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