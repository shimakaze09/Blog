using FreeSql.DataAnnotations;

namespace Data.Models;

/// <summary>
/// Top post
/// </summary>
[Table(Name = "top_post", OldName = "TopPost")]
public class TopPost
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string PostId { get; set; }
    public Post Post { get; set; }
}