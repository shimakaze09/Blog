using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace Data.Models;

public class Post
{
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    ///     Title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Article link. Once set, the article can be accessed in the following format:
    ///     <para> http://myblog.com/p/post-slug1 </para>
    /// </summary>
    [MaxLength(150)]
    public string? Slug { get; set; }

    /// <summary>
    ///     Article tag, extracted from the prefix of the original Markdown file, used to distinguish article status. Examples:
    ///     <para> "Blog title" </para>
    /// </summary>
    public string? Status { get; set; }

    // TODO: add tagging functionality

    /// <summary>
    ///     Whether to publish (if not published, it is a draft).
    /// </summary>
    public bool IsPublish { get; set; }

    /// <summary>
    ///     Summary
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    ///     Content (Markdown format)
    /// </summary>
    [MaxLength(-1)]
    public string? Content { get; set; }

    /// <summary>
    ///     The relative path of the blog post before importing.
    ///     <para>For example: "Series/AspNetCoreDevelopmentNotes"</para>
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     Creation Time
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    ///     Last Modified Time
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    ///     Category ID
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    ///     Category
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    ///     Hierarchical levels of the article's categories. The content is formatted like this: `1,2,3`, with category IDs
    ///     separated by commas.
    /// </summary>
    public string? Categories { get; set; }
}