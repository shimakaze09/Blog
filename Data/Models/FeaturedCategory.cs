using FreeSql.DataAnnotations;

namespace Data.Models;

/// <summary>
///     Featured category
/// </summary>
public class FeaturedCategory
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    /// <summary>
    ///     The redefined name of the recommendation.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Explanation of the recommendation category.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Icon for the recommendation.
    ///     <list type="number">
    ///         <listheader>Examples</listheader>
    ///         <item>fa-solid fa-c</item>
    ///         <item>fa-brands fa-python</item>
    ///         <item>fa-brands fa-android</item>
    ///     </list>
    /// </summary>
    public string IconCssClass { get; set; }
}