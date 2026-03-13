using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class RequestPasswordResetRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;
}
