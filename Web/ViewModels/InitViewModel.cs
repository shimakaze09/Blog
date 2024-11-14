using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels;

public class InitViewModel
{
    [Display(Name = "Username")] public string Username { get; set; }

    [Display(Name = "Password")] public string Password { get; set; }

    [Display(Name = "Blog Host")] public string Host { get; set; }

    [Display(Name = "Default Render Method for Articles")]
    public string DefaultRender { get; set; }
}