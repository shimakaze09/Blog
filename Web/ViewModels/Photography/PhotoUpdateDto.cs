using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Photography;

public class PhotoUpdateDto
{
    public string? Id { get; set; }

    /// <summary>
    ///     Work Title
    /// </summary>
    [Required(ErrorMessage = "Work title cannot be empty")]
    public string Title { get; set; }

    /// <summary>
    ///     Shooting location
    /// </summary>
    [Required(ErrorMessage = "Shooting location cannot be empty")]
    public string Location { get; set; }

    public DateTime CreateTime { get; set; }
}