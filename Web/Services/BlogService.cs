using Data.Models;
using FreeSql;

namespace Web.Services;

public class BlogService
{
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;
    private readonly IBaseRepository<TopPost> _topPostRepo;

    public BlogService(IBaseRepository<TopPost> topPostRepo, IBaseRepository<FeaturedPost> featuredPostRepo)
    {
        _topPostRepo = topPostRepo;
        _featuredPostRepo = featuredPostRepo;
    }

    public Post? GetTopOnePost()
    {
        return _topPostRepo.Select.Include(a => a.Post.Category).First()?.Post;
    }

    /// <summary>
    ///     Get recommended blog rows, with a maximum of two blogs per row
    /// </summary>
    /// <returns></returns>
    [Obsolete("No need to separate rows anymore, use GetFeaturedPosts() directly")]
    public List<List<Post>> GetFeaturedPostRows()
    {
        var data = new List<List<Post>>();

        var posts = GetFeaturedPosts();
        for (var i = 0; i < posts.Count; i += 2) data.Add(new List<Post> { posts[i], posts[i + 1] });

        return data;
    }

    public List<Post> GetFeaturedPosts()
    {
        return _featuredPostRepo.Select.Include(a => a.Post.Category)
            .ToList(a => a.Post);
    }

    public FeaturedPost AddFeaturedPost(Post post)
    {
        var item = _featuredPostRepo.Where(a => a.PostId == post.Id).First();
        if (item != null) return item;
        item = new FeaturedPost { PostId = post.Id };
        _featuredPostRepo.Insert(item);
        return item;
    }

    public int DeleteFeaturedPost(Post post)
    {
        var item = _featuredPostRepo.Where(a => a.PostId == post.Id).First();
        return item == null ? 0 : _featuredPostRepo.Delete(item);
    }

    /// <summary>
    ///     Set the top blog
    /// </summary>
    /// <param name="post"></param>
    /// <returns>Returns the <see cref="TopPost" /> object and the number of rows deleted for the existing top blog</returns>
    public (TopPost, int) SetTopPost(Post post)
    {
        var rows = _topPostRepo.Select.ToDelete().ExecuteAffrows();
        var item = new TopPost { PostId = post.Id };
        _topPostRepo.Insert(item);
        return (item, rows);
    }
}