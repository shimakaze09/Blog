namespace Web.ViewModels.Blog;

/// <summary>
///     Blog Overview
/// </summary>
public class BlogOverview
{
    /// <summary>
    ///     Number of posts
    /// </summary>
    public long PostsCount { get; set; }

    /// <summary>
    ///     Number of categories
    /// </summary>
    public long CategoriesCount { get; set; }

    /// <summary>
    ///     Number of photos
    /// </summary>
    public long PhotosCount { get; set; }

    /// <summary>
    ///     Number of featured posts
    /// </summary>
    public long FeaturedPostsCount { get; set; }

    /// <summary>
    ///     Number of featured categories
    /// </summary>
    public long FeaturedCategoriesCount { get; set; }

    /// <summary>
    ///     Number of featured photos
    /// </summary>
    public long FeaturedPhotosCount { get; set; }
}