using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Reset token is required")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
