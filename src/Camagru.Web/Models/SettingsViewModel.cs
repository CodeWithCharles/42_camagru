namespace Camagru.Web.Models;

public class SettingsViewModel
{
    public string Email { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; }
}