using FreeSql.DataAnnotations;

namespace Data.Models;

/// <summary>
/// Visit Record
/// </summary>
[Table(Name = "visit_record")]
public class VisitRecord 
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    /// <summary>
    /// IP Address
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// Request Path
    /// </summary>
    public string RequestPath { get; set; }

    /// <summary>
    /// Request Query String
    /// </summary>
    public string? RequestQueryString { get; set; }

    /// <summary>
    /// Request Method
    /// </summary>
    public string RequestMethod { get; set; }

    /// <summary>
    /// User Agent
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Time { get; set; }
}
