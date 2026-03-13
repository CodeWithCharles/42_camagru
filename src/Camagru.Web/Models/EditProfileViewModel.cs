namespace Camagru.Web.Models;

public class EditProfileViewModel
{
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string Email { get; set; } = string.Empty;
}
