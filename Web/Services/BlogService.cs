using System.IO.Compression;
using System.Text;
using Data.Models;
using FreeSql;
using Share;
using Share.Utils;
using Web.ViewModels.Blog;

namespace Web.Services;

public class BlogService
{
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IWebHostEnvironment _environment;
    private readonly IBaseRepository<FeaturedCategory> _fCategoryRepo;
    private readonly IBaseRepository<FeaturedPhoto> _fPhotoRepo;
    private readonly IBaseRepository<FeaturedPost> _fPostRepo;
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<TopPost> _topPostRepo;

    public BlogService(IBaseRepository<TopPost> topPostRepo, IBaseRepository<FeaturedPost> fPostRepo,
        IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo, IBaseRepository<Photo> photoRepo,
        IBaseRepository<FeaturedCategory> fCategoryRepo,
        IBaseRepository<FeaturedPhoto> fPhotoRepo, IWebHostEnvironment environment)
    {
        _topPostRepo = topPostRepo;
        _fPostRepo = fPostRepo;
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _photoRepo = photoRepo;
        _fCategoryRepo = fCategoryRepo;
        _fPhotoRepo = fPhotoRepo;
        _environment = environment;
    }

    /// <summary>
    ///     Get blog overview information
    /// </summary>
    /// <returns></returns>
    public async Task<BlogOverview> Overview()
    {
        return new BlogOverview
        {
            PostsCount = await _postRepo.Select.CountAsync(),
            CategoriesCount = await _categoryRepo.Select.CountAsync(),
            PhotosCount = await _photoRepo.Select.CountAsync(),
            FeaturedPostsCount = await _fPostRepo.Select.CountAsync(),
            FeaturedCategoriesCount = await _fCategoryRepo.Select.CountAsync(),
            FeaturedPhotosCount = await _fPhotoRepo.Select.CountAsync()
        };
    }

    public async Task<Post?> GetTopOnePost()
    {
        return (await _topPostRepo.Select.Include(a => a.Post.Category).FirstAsync())?.Post;
    }

    /// <summary>
    ///     Get featured blog rows, with a maximum of two blogs per row
    /// </summary>
    /// <returns></returns>
    [Obsolete("No need to separate into rows, just use GetFeaturedPosts()")]
    public async Task<List<List<Post>>> GetFeaturedPostRows()
    {
        var data = new List<List<Post>>();

        var posts = await GetFeaturedPosts();
        for (var i = 0; i < posts.Count; i += 2) data.Add(new List<Post> { posts[i], posts[i + 1] });

        return data;
    }

    public async Task<List<Post>> GetFeaturedPosts()
    {
        return await _fPostRepo.Select.Include(a => a.Post.Category)
            .ToListAsync(a => a.Post);
    }

    public async Task<FeaturedPost> AddFeaturedPost(Post post)
    {
        var item = await _fPostRepo.Where(a => a.PostId == post.Id).FirstAsync();
        if (item != null) return item;
        item = new FeaturedPost { PostId = post.Id };
        await _fPostRepo.InsertAsync(item);
        return item;
    }

    public async Task<int> DeleteFeaturedPost(Post post)
    {
        var items = await _fPostRepo.Where(a => a.PostId == post.Id).CountAsync();
        return items == 0 ? 0 : await _fPostRepo.Where(a => a.PostId == post.Id).ToDelete().ExecuteAffrowsAsync();
    }

    /// <summary>
    ///     Set top blog post
    /// </summary>
    /// <param name="post"></param>
    /// <returns>Returns <see cref="TopPost" /> object and the number of rows deleted for the original top blog post</returns>
    public async Task<(TopPost, int)> SetTopPost(Post post)
    {
        var rows = await _topPostRepo.Select.ToDelete().ExecuteAffrowsAsync();
        var item = new TopPost { PostId = post.Id };
        await _topPostRepo.InsertAsync(item);
        return (item, rows);
    }

    /// <summary>
    ///     Get the list of post statuses
    /// </summary>
    /// <returns></returns>
    public async Task<List<string?>> GetStatusList()
    {
        return await _postRepo.Select.GroupBy(a => a.Status)
            .ToListAsync(a => a.Key);
    }

    /// <summary>
    ///     Upload blog post
    ///     todo This function is initially completed, but there is too much redundant code, needs optimization
    /// </summary>
    /// <returns></returns>
    public async Task<Post> Upload(PostCreationDto dto, IFormFile file)
    {
        var tempFile = Path.GetTempFileName();
        await using (var fs = new FileStream(tempFile, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        var extractPath = Path.Combine(Path.GetTempPath(), "StarBlog", Guid.NewGuid().ToString());
        // Use GBK encoding to extract, to prevent Chinese file name garbling
        // Reference: https://www.cnblogs.com/liguix/p/11883248.html
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.GetEncoding("GBK"));

        var dir = new DirectoryInfo(extractPath);
        var files = dir.GetFiles("*.md");
        var mdFile = files.First();

        using var reader = mdFile.OpenText();
        var content = await reader.ReadToEndAsync();
        var post = new Post
        {
            Id = GuidUtils.GuidTo16String(),
            Status = "Published",
            Title = dto.Title ?? $"{DateTime.Now.ToLongDateString()} Article",
            Summary = dto.Summary,
            IsPublish = true,
            Content = content,
            Path = "",
            CreationTime = DateTime.Now,
            LastUpdateTime = DateTime.Now,
            CategoryId = dto.CategoryId
        };

        // Handle multi-level categories
        var category = await _categoryRepo.Where(a => a.Id == dto.CategoryId).FirstAsync();
        if (category == null)
        {
            post.Categories = "0";
        }
        else
        {
            var categories = new List<Category> { category };
            var parent = category.Parent;
            while (parent != null)
            {
                categories.Add(parent);
                parent = parent.Parent;
            }

            categories.Reverse();
            post.Categories = string.Join(",", categories.Select(a => a.Id));
        }

        var assetsPath = Path.Combine(_environment.WebRootPath, "media", "blog");
        var processor = new PostProcessor(extractPath, assetsPath, post);

        // Process article title and status
        processor.InflateStatusTitle();

        // Process article content
        // When importing articles, import images within the article and perform relative path replacement for images
        post.Content = processor.MarkdownParse();
        if (string.IsNullOrEmpty(post.Summary)) post.Summary = processor.GetSummary(200);

        // Save to database
        post = await _postRepo.InsertAsync(post);

        return post;
    }
}