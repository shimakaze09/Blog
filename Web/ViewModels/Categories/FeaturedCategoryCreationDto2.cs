namespace Web.ViewModels.Categories;

public class FeaturedCategoryCreationDto2
{
    /// <summary>
    ///     Redefined featured name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Featured category description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Icon
    ///     <list type="number">
    ///         <listheader>Example</listheader>
    ///         <item>fa-solid fa-c</item>
    ///         <item>fa-brands fa-python</item>
    ///         <item>fa-brands fa-android</item>
    ///     </list>
    /// </summary>
    public string IconCssClass { get; set; }

    /// <summary>
    ///     Category ID
    /// </summary>
    public int CategoryId { get; set; }
}