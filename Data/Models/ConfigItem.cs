using FreeSql.DataAnnotations;

namespace Data.Models;

/// <summary>
/// Configure Item
/// </summary>
public class ConfigItem
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string Key { get; set; }
    public string Value { get; set; }
    public string? Description { get; set; }
}