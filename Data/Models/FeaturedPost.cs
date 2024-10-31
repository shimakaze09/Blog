using FreeSql.DataAnnotations;

namespace Data.Models;

/// <summary>
///     Featured post
/// </summary>
[Table(Name = "featured_post", OldName = "FeaturedPost")]
public class FeaturedPost
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string PostId { get; set; }
    public Post Post { get; set; }
}