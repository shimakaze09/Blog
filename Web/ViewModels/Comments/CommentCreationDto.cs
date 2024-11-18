using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.Comments;

public class CommentCreationDto
{
    public string? ParentId { get; set; }
    [Required] public string PostId { get; set; }

    [MinLength(2, ErrorMessage = "Length must be between 2 and 20 characters")]
    [MaxLength(20, ErrorMessage = "Length must be between 2 and 20 characters")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email address cannot be empty")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string? Url { get; set; }

    [Required(ErrorMessage = "Email OTP cannot be empty")]
    [StringLength(4, ErrorMessage = "Length must be 4 characters")]
    public string EmailOtp { get; set; }

    [Required(ErrorMessage = "Comment content cannot be empty")]
    [MaxLength(300, ErrorMessage = "Comment maximum length is 300 characters")]
    public string Content { get; set; }
}