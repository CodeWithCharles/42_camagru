using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class ChangeEmailRequest
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string NewEmail { get; set; } = string.Empty;
}
