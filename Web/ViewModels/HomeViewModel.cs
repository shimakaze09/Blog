using Data.Models;

namespace Web.ViewModels;

public class HomeViewModel
{
    /// <summary>
    ///     Random photo
    /// </summary>
    public Photo? RandomPhoto { get; set; }

    /// <summary>
    ///     Whether to display Chart
    /// </summary>
    public bool ChartVisible { get; set; } = true;

    /// <summary>
    ///     Whether to display random images
    /// </summary>
    public bool RandomPhotoVisible { get; set; } = false;

    /// <summary>
    ///     Featured blog posts
    /// </summary>
    public Post? TopPost { get; set; }

    /// <summary>
    ///     Recommended blogs, with a maximum of two blogs per row
    /// </summary>
    public List<Post> FeaturedPosts { get; set; } = new();

    /// <summary>
    ///     Recommended photos. Generally, there should be only three.
    /// </summary>
    public List<Photo> FeaturedPhotos { get; set; } = new();

    /// <summary>
    ///     Recommended categories. Generally, there should be three.
    /// </summary>
    public List<FeaturedCategory> FeaturedCategories { get; set; } = new();

    /// <summary>
    ///     Friend links
    /// </summary>
    public List<Link> Links { get; set; } = new();

    /// <summary>
    ///     Poem
    /// </summary>
    public string OnePoem { get; set; }

    /// <summary>
    ///     Hitokoto
    /// </summary>
    public string Hitokoto { get; set; }
}