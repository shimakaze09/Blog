using Data.Models;
using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Share.Extensions.Markdown;
using Share.Utils;
using Web.ViewModels;
using Web.ViewModels.QueryFilters;
using X.PagedList;

namespace Web.Services;

public class PostService
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly CommonService _commonService;
    private readonly ConfigService _conf;
    private readonly IWebHostEnvironment _environment;
    private readonly LinkGenerator _generator;
    private readonly ILogger<PostService> _logger;
    private readonly IBaseRepository<Post> _postRepo;

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IWebHostEnvironment environment,
        IHttpContextAccessor accessor,
        LinkGenerator generator,
        ConfigService conf,
        CommonService commonService,
        ILogger<PostService> logger)
    {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _environment = environment;
        _accessor = accessor;
        _generator = generator;
        _conf = conf;
        _commonService = commonService;
        _logger = logger;
    }

    private string Host => _conf["host"];

    /// <summary>
    ///     Checks if the given slug is available
    /// </summary>
    public async Task<bool> CheckSlugAvailable(string slug)
    {
        return !await _postRepo.Select.AnyAsync(a => a.Slug == slug);
    }

    public async Task<Post?> GetById(string id)
    {
        // When retrieving the post, parse the image URLs in the markdown content and add the full URL before returning to the frontend
        var post = await _postRepo.Where(a => a.Id == id).Include(a => a.Category).FirstAsync();
        if (post != null) post.Content = MdImageLinkConvert(post);

        return post;
    }

    public Task<int> Delete(string id)
    {
        return _postRepo.DeleteAsync(a => a.Id == id);
    }

    public async Task<Post> InsertOrUpdateAsync(Post post)
    {
        var postId = post.Id;
        // If it's a new post, save it to the database first
        if (await _postRepo.Where(a => a.Id == postId).CountAsync() == 0) post = await _postRepo.InsertAsync(post);

        // Check for external images in the post content, download and replace them
        // TODO: Move the external image download to an asynchronous task to avoid slowing down the post save process
        post.Content = await MdExternalUrlDownloadAsync(post);
        // When updating the post, replace the image URLs in the markdown content with relative paths before saving
        post.Content = MdImageLinkConvert(post, false);

        // Update the post content again after processing
        await _postRepo.UpdateAsync(post);
        return post;
    }

    /// <summary>
    ///     Upload an image for a specific post
    /// </summary>
    public async Task<string> UploadImage(Post post, IFormFile file)
    {
        InitPostMediaDir(post);

        // Directly generate a unique filename without retaining the original filename
        var filename = GuidUtils.GuidTo16String() + Path.GetExtension(file.FileName);
        var fileRelativePath = Path.Combine("media", "blog", post.Id, filename);
        var savePath = Path.Combine(_environment.WebRootPath, fileRelativePath);

        await using (var fs = new FileStream(savePath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        return Path.Combine(Host, fileRelativePath);
    }

    /// <summary>
    ///     Get the images for a specific post
    /// </summary>
    public List<string> GetImages(Post post)
    {
        var data = new List<string>();
        var postDir = InitPostMediaDir(post);
        foreach (var file in Directory.GetFiles(postDir))
            data.Add(Path.Combine(Host, "media", "blog", post.Id, Path.GetFileName(file)));

        return data;
    }

    public async Task<IPagedList<Post>> GetPagedList(PostQueryParameters param, bool adminMode = false)
    {
        var querySet = _postRepo.Select;

        // Filter published status
        // Only admins can filter by published status
        if (param.IsPublish != null && adminMode)
            querySet = _postRepo.Select.Where(a => a.IsPublish == param.IsPublish);

        if (!adminMode) querySet = _postRepo.Select.Where(a => a.IsPublish);


        // Filter by status
        if (!string.IsNullOrWhiteSpace(param.Status)) querySet = querySet.Where(a => a.Status == param.Status);

        // Filter by category
        if (param.CategoryId != 0) querySet = querySet.Where(a => a.CategoryId == param.CategoryId);

        // Filter by keywords
        if (!string.IsNullOrWhiteSpace(param.Search)) querySet = querySet.Where(a => a.Title.Contains(param.Search));

        // Sort
        if (!string.IsNullOrWhiteSpace(param.SortBy))
        {
            // Determine if ascending order
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');

            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        IPagedList<Post> pagedList = new StaticPagedList<Post>(
            await querySet.Page(param.Page, param.PageSize).Include(a => a.Category).ToListAsync(),
            param.Page, param.PageSize, Convert.ToInt32(await querySet.CountAsync())
        );
        return pagedList;
    }

    /// <summary>
    ///     Convert a Post object to a PostViewModel object
    /// </summary>
    public async Task<PostViewModel> GetPostViewModel(Post post, bool md2Html = true)
    {
        var model = new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary ?? "(No summary)",
            Content = post.Content ?? "(No content)",
            Path = post.Path ?? string.Empty,
            Url = _generator.GetUriByAction(
                _accessor.HttpContext!,
                "Post", "Blog",
                new { post.Id }
            ),
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            Category = post.Category,
            Categories = new List<Category>(),
            TocNodes = post.ExtractToc()
        };

        if (!string.IsNullOrWhiteSpace(post.Slug))
            model.Url = Host + _generator.GetPathByAction(
                _accessor.HttpContext!,
                "PostBySlug", "Blog",
                new { post.Slug }
            );

        if (md2Html) model.ContentHtml = GetContentHtml(post);

        if (post.Categories != null)
            foreach (var itemId in post.Categories.Split(",").Select(int.Parse))
            {
                var item = await _categoryRepo.Where(a => a.Id == itemId).FirstAsync();
                if (item != null) model.Categories.Add(item);
            }

        return model;
    }

    public static string GetContentHtml(Post post)
    {
        // TODO: Research backend rendering of Markdown (PS: Although frontend rendering has more tools and better effects, backend rendering won't feel disjointed)
        // Some reference materials for this part:
        // - About frontend rendering Markdown styles: https://blog.csdn.net/sprintline/article/details/122849907
        // - https://github.com/showdownjs/showdown
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseBootstrap5()
            .Build();
        return Markdown.ToHtml(post.Content ?? "", pipeline);
    }

    /// <summary>
    ///     Initialize the resource directory for a blog post
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
    ///     Convert image links in Markdown content
    ///     <para>Supports adding or removing URL prefixes in Markdown image URLs</para>
    /// </summary>
    /// <param name="post"></param>
    /// <param name="isAddPrefix">Whether to add the full URL prefix of this site</param>
    /// <returns></returns>
    private string MdImageLinkConvert(Post post, bool isAddPrefix = true)
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

                // Skip if the URL already has the Host prefix
                if (isAddPrefix && imgUrl.StartsWith(Host)) continue;

                // Set the full URL
                if (isAddPrefix)
                {
                    if (imgUrl.StartsWith("http")) continue;
                    linkInline.Url = $"{Host}/media/blog/{post.Id}/{imgUrl}";
                }
                // Set to relative URL
                else
                {
                    linkInline.Url = Path.GetFileName(imgUrl);
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    ///     Download external images in Markdown content
    ///     <para>If the Markdown content contains external image URLs, download them locally and replace the URLs</para>
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    private async Task<string> MdExternalUrlDownloadAsync(Post post)
    {
        if (post.Content == null) return string.Empty;

        // Initialize the directory first
        InitPostMediaDir(post);

        var document = Markdown.Parse(post.Content);
        foreach (var node in document.AsEnumerable())
        {
            if (node is not ParagraphBlock { Inline: not null } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline)
            {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                // Skip empty URLs
                if (imgUrl == null) continue;
                // Skip images with the Host prefix
                if (imgUrl.StartsWith(Host)) continue;

                // Download the image
                _logger.LogDebug("Post: {Title}, downloading image: {Url}", post.Title, imgUrl);
                var savePath = Path.Combine(_environment.WebRootPath, "media", "blog", post.Id!);
                var fileName = await _commonService.DownloadFileAsync(imgUrl, savePath);
                linkInline.Url = fileName;
            }
        }

        await using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }
}