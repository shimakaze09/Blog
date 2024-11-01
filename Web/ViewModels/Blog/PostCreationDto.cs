namespace Web.ViewModels.Blog;

public class PostCreationDto
{
    /// <summary>
    ///     Title
    /// </summary>
    public string? Title { get; set; }

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
}