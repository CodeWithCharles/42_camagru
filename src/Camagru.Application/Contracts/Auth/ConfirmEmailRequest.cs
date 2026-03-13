using System.ComponentModel.DataAnnotations;

namespace Camagru.Application.Contracts.Auth;

public class ConfirmEmailRequest
{
    [Required(ErrorMessage = "Confirmation token is required")]
    public string Token { get; set; } = string.Empty;
}
