using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Display name must not exceed 30 characters")]
    public string? DisplayName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Bio must not exceed 500 characters")]
    public string? Bio { get; set; }
}
