using Data.Models;

namespace Web.ViewModels;

public class HomeViewModel
{
    /// <summary>
    /// Featured blog posts
    /// </summary>
    public Post? TopPost { get; set; }

    /// <summary>
    /// Recommended blogs, with a maximum of two blogs per row
    /// </summary>
    public List<List<Post>> FeaturedPosts { get; set; }=new();
}
