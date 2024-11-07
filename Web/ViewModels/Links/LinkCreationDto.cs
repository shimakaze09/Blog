namespace Web.ViewModels.Links;

public class LinkCreationDto
{
    /// <summary>
    ///     Website Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Is Visible
    /// </summary>
    public bool Visible { get; set; }
}