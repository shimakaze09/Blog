using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.LinkExchange;

public class LinkExchangeAddViewModel
{
    /// <summary>
    ///     Website Name
    /// </summary>
    [Display(Name = "Website Name")]
    [Required(ErrorMessage = "Website name is required")]
    public string Name { get; set; }

    /// <summary>
    ///     Description
    /// </summary>
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    ///     URL
    /// </summary>
    [Display(Name = "URL")]
    [Required(ErrorMessage = "URL is required")]
    [DataType(DataType.Url)]
    public string Url { get; set; }

    /// <summary>
    ///     Webmaster Name
    /// </summary>
    [Display(Name = "Webmaster Name")]
    [Required(ErrorMessage = "Webmaster name is required")]
    public string WebMaster { get; set; }

    /// <summary>
    ///     Contact Email
    /// </summary>
    [Display(Name = "Contact Email")]
    [Required(ErrorMessage = "Contact email is required")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
}