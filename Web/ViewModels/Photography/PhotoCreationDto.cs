using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Photography;

public class PhotoCreationDto
{
    /// <summary>
    ///     Photo title
    /// </summary>
    [Required(ErrorMessage = "Photo title cannot be empty")]
    public string Title { get; set; }

    /// <summary>
    ///     Shooting location
    /// </summary>
    [Required(ErrorMessage = "Shooting location cannot be empty")]
    public string Location { get; set; }
}