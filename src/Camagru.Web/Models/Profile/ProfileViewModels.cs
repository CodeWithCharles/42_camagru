using System.ComponentModel.DataAnnotations;

namespace Camagru.Web.Models.Profile;

public class ProfilePageViewModel
{
    public ProfileSummaryViewModel Summary { get; set; } = new();
    public UpdateProfileFormViewModel Details { get; set; } = new();
    public ChangeEmailFormViewModel EmailChange { get; set; } = new();
    public ChangePasswordFormViewModel PasswordChange { get; set; } = new();
    public NotificationPreferencesFormViewModel Preferences { get; set; } = new();
    public DeleteAccountFormViewModel DeleteAccount { get; set; } = new();
    public IReadOnlyList<ProfilePostPreviewViewModel> RecentPosts { get; set; } = [];
    public string ActiveSection { get; set; } = "overview";
}

public class ProfileSummaryViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PostCount { get; set; }
}

public class ProfilePostPreviewViewModel
{
    public int PostId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedLabel { get; set; } = string.Empty;
}

public class UpdateProfileFormViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [MaxLength(30, ErrorMessage = "Display name must not exceed 30 characters")]
    public string? DisplayName { get; set; }

    [MaxLength(500, ErrorMessage = "Bio must not exceed 500 characters")]
    public string? Bio { get; set; }

    [Required]
    [EmailAddress]
    public string CurrentEmail { get; set; } = string.Empty;
}

public class ChangeEmailFormViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string NewEmail { get; set; } = string.Empty;
}

public class ChangePasswordFormViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string NewPassword { get; set; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class NotificationPreferencesFormViewModel
{
    public bool EmailNotificationsEnabled { get; set; }
}

public class DeleteAccountFormViewModel
{
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
