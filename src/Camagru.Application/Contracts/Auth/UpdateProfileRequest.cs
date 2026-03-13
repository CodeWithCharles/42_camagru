using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string? ConfirmNewPassword { get; set; }
}
