using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class ResendConfirmationEmailRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
}
