namespace Web.ViewModels.Blog;

public class PostCreationDto
{
    /// <summary>
    ///     Title
    /// </summary>
    public string? Title { get; set; }

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
    ///     Summary
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    ///     Content (in Markdown format)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    ///     Category ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    ///     ZIP encoding
    /// </summary>
    public string ZipEncoding { get; set; } = "utf-8";
}