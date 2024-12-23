namespace Web.ViewModels.Blog;

public class PostUpdateDto
{
    public string Id { get; set; }

    /// <summary>
    ///     Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Article link, can be accessed via the following format
    ///     <para> http://blog.com/p/post-slug1 </para>
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    ///     Article Tag
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    ///     Indicates whether the item is published (not published means it's a draft).
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    ///     Summary
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    ///     Content (in markdown format)
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     Category ID
    /// </summary>
    public int CategoryId { get; set; }
}