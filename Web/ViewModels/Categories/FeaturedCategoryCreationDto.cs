namespace Web.ViewModels.Categories;

public class FeaturedCategoryCreationDto
{
    /// <summary>
    ///     Redefined featured category name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Explanation of the featured category
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Icon
    ///     <list type="number">
    ///         <listheader>Example</listheader>
    ///         <item>fa-solid fa-globe</item>
    ///         <item>fa-solid fa-c</item>
    ///         <item>fa-brands fa-android</item>
    ///     </list>
    /// </summary>
    public string IconCssClass { get; set; }
}