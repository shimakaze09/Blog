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
    private readonly IHttpContextAccessor _accessor;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly CommonService _commonService;
    private readonly ConfigService _conf;
    private readonly IWebHostEnvironment _environment;
    private readonly LinkGenerator _generator;
    private readonly IBaseRepository<Post> _postRepo;

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IWebHostEnvironment environment,
        IHttpContextAccessor accessor,
        LinkGenerator generator,
        ConfigService conf,
        CommonService commonService)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
        _accessor = accessor;
        _generator = generator;
        _conf = conf;
        _commonService = commonService;
    }

    public string Host => _conf["host"];

    public Post? GetById(string id)
    {
        var post = _postRepo.Where(a => a.Id == id).Include(a => a.Category).First();
        if (post != null) post.Content = MdImageLinkConvert(post);
        return post;
    }

    public int Delete(string id)
    {
        return _postRepo.Delete(a => a.Id == id);
    }

    public Post InsertOrUpdate(Post post)
    {
        post.Content = MdImageLinkConvert(post, false);
        return _postRepo.InsertOrUpdate(post);
    }

    /// <summary>
    ///     Upload images for a specific post
    /// </summary>
    /// <param name="post"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public string UploadImage(Post post, IFormFile file)
    {
        InitPostMediaDir(post);

        var filename = WebUtility.UrlEncode(file.FileName);
        var fileRelativePath = Path.Combine("media", "blog", post.Id, filename);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);
        if (File.Exists(savePath))
        {
            // Handle file rename
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
    ///     Get images for a specific post
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
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

        // Filter published posts
        if (param.OnlyPublished) querySet = _postRepo.Select.Where(a => a.IsPublish);

        // Apply status filter
        if (!string.IsNullOrEmpty(param.Status)) querySet = querySet.Where(a => a.Status == param.Status);

        // Apply category filter
        if (param.CategoryId != 0) querySet = querySet.Where(a => a.CategoryId == param.CategoryId);

        // Apply keyword filter
        if (!string.IsNullOrEmpty(param.Search)) querySet = querySet.Where(a => a.Title.Contains(param.Search));

        // Apply sorting
        if (!string.IsNullOrEmpty(param.SortBy))
        {
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
            // TODO Research backend rendering of Markdown
            ContentHtml = Markdown.ToHtml(post.Content),
            Path = post.Path,
            Url = _generator.GetUriByAction(
                _accessor.HttpContext!,
                "Post", "Blog",
                new { post.Id }
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
    ///     Initialize the resource directory for blog posts
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    private string InitPostMediaDir(Post post)
    {
        var blogMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id);
        if (!Directory.Exists(blogMediaDir)) Directory.CreateDirectory(blogMediaDir);
        if (!Directory.Exists(postMediaDir)) Directory.CreateDirectory(postMediaDir);

        return postMediaDir;
    }

    /// <summary>
    ///     <para>Convert Markdown image links</para>
    ///     <list type="number">
    ///         <listheader>Function</listheader>
    ///         <item>Support adding or removing URL prefixes from Markdown image links</item>
    ///         <item>If Markdown contains external image URLs, download them locally and replace the URLs</item>
    ///     </list>
    /// </summary>
    /// <param name="post"></param>
    /// <param name="isAddPrefix">Whether to add the full URL prefix of the site</param>
    /// <param name="isDownloadExternalUrl">Whether to download images with external URLs</param>
    /// <returns></returns>
    private string MdImageLinkConvert(Post post, bool isAddPrefix = true, bool isDownloadExternalUrl = false)
    {
        if (post.Content == null) return string.Empty;
        var document = Markdown.Parse(post.Content);

        foreach (var node in document.AsEnumerable())
        {
            if (node is not ParagraphBlock { Inline: not null } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline)
            {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;

                // Already has Host prefix, skip
                if (isAddPrefix && imgUrl.StartsWith(Host)) continue;

                // Set full link
                if (isAddPrefix)
                {
                    if (imgUrl.StartsWith("http")) continue;
                    linkInline.Url = $"{Host}/media/blog/{post.Id}/{imgUrl}";
                }
                // Set relative link
                else
                {
                    if (!isDownloadExternalUrl)
                    {
                        linkInline.Url = Path.GetFileName(imgUrl);
                        continue;
                    }

                    // Download image
                    var savePath = Path.Combine(_environment.WebRootPath);
                    // TODO: Complete image download logic
                    // var fileName= await _commonService.DownloadFileAsync(imgUrl, savePath);
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}