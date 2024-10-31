using FreeSql.DataAnnotations;

namespace Data.Models;

public class Photo
{
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    ///     Title of the photo
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    ///     Location where the photo was taken
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    ///     File path where the photo is stored
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    ///     Height of the photo in pixels
    /// </summary>
    public long Height { get; set; }

    /// <summary>
    ///     Width of the photo in pixels
    /// </summary>
    public long Width { get; set; }

    /// <summary>
    ///     Creation time of the photo
    /// </summary>
    public DateTime CreateTime { get; set; }
}