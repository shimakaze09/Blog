using Data.Models;
using FreeSql;
using Web.ViewModels.Blog;

namespace Web.Services;

public class BlogService
{
    private readonly IBaseRepository<Category> _categoryRepo;
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
        IBaseRepository<FeaturedPhoto> fPhotoRepo)
    {
        _topPostRepo = topPostRepo;
        _fPostRepo = fPostRepo;
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _photoRepo = photoRepo;
        _fCategoryRepo = fCategoryRepo;
        _fPhotoRepo = fPhotoRepo;
    }

    /// <summary>
    ///     Get blog overview information
    /// </summary>
    /// <returns></returns>
    public BlogOverview Overview()
    {
        return new BlogOverview
        {
            PostsCount = _postRepo.Select.Count(),
            CategoriesCount = _categoryRepo.Select.Count(),
            PhotosCount = _photoRepo.Select.Count(),
            FeaturedPostsCount = _fPostRepo.Select.Count(),
            FeaturedCategoriesCount = _fCategoryRepo.Select.Count(),
            FeaturedPhotosCount = _fPhotoRepo.Select.Count()
        };
    }

    public Post? GetTopOnePost()
    {
        return _topPostRepo.Select.Include(a => a.Post.Category).First()?.Post;
    }

    /// <summary>
    ///     Get recommended blog rows, each row contains at most two blogs
    /// </summary>
    /// <returns></returns>
    [Obsolete("No longer needed to separate into rows, use GetFeaturedPosts() directly")]
    public List<List<Post>> GetFeaturedPostRows()
    {
        var data = new List<List<Post>>();

        var posts = GetFeaturedPosts();
        for (var i = 0; i < posts.Count; i += 2) data.Add(new List<Post> { posts[i], posts[i + 1] });

        return data;
    }

    public List<Post> GetFeaturedPosts()
    {
        return _fPostRepo.Select.Include(a => a.Post.Category)
            .ToList(a => a.Post);
    }

    public FeaturedPost AddFeaturedPost(Post post)
    {
        var item = _fPostRepo.Where(a => a.PostId == post.Id).First();
        if (item != null) return item;
        item = new FeaturedPost { PostId = post.Id };
        _fPostRepo.Insert(item);
        return item;
    }

    public int DeleteFeaturedPost(Post post)
    {
        var item = _fPostRepo.Where(a => a.PostId == post.Id).First();
        return item == null ? 0 : _fPostRepo.Delete(item);
    }

    /// <summary>
    ///     Set a blog as featured
    /// </summary>
    /// <param name="post"></param>
    /// <returns>Return TopPost object and the number of deleted original featured blog rows</returns>
    public (TopPost, int) SetTopPost(Post post)
    {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        var item = new TopPost { PostId = post.Id };
        _topPostRepo.Insert(item);
        return (item, rows);
    }

    /// <summary>
    ///     Get list of article statuses
    /// </summary>
    /// <returns>A list of nullable string values representing article statuses</returns>
    public List<string?> GetStatusList()
    {
        return _postRepo.Select.GroupBy(a => a.Status)
            .ToList(a => a.Key);
    }
}