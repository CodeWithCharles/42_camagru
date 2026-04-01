using System.Security.Claims;
using Camagru.Application.Contracts.Auth;
using Camagru.Application.UseCases.Auth;
using Camagru.Web.Models.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class ProfileController : Controller
{
    private readonly GetUserProfileUseCase _getUserProfileUseCase;
    private readonly UpdateProfileUseCase _updateProfileUseCase;
    private readonly ChangePasswordUseCase _changePasswordUseCase;
    private readonly ChangeEmailUseCase _changeEmailUseCase;
    private readonly DeleteAccountUseCase _deleteAccountUseCase;
    private readonly UpdateNotificationPreferencesUseCase _updateNotificationPreferencesUseCase;

    public ProfileController(
        GetUserProfileUseCase getUserProfileUseCase,
        UpdateProfileUseCase updateProfileUseCase,
        ChangePasswordUseCase changePasswordUseCase,
        ChangeEmailUseCase changeEmailUseCase,
        DeleteAccountUseCase deleteAccountUseCase,
        UpdateNotificationPreferencesUseCase updateNotificationPreferencesUseCase)
    {
        _getUserProfileUseCase = getUserProfileUseCase;
        _updateProfileUseCase = updateProfileUseCase;
        _changePasswordUseCase = changePasswordUseCase;
        _changeEmailUseCase = changeEmailUseCase;
        _deleteAccountUseCase = deleteAccountUseCase;
        _updateNotificationPreferencesUseCase = updateNotificationPreferencesUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            TempData["Toast.Info"] = "Log in to manage your profile and account settings.";
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Profile") });
        }

        var model = await BuildProfilePageAsync(userId);
        if (model == null)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        return View(model);
    }

    [HttpPost("UpdateDetails")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDetails([Bind(Prefix = "Details")] UpdateProfileFormViewModel form)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Profile") });
        }

        if (!ModelState.IsValid)
        {
            return await RenderProfileWithSectionAsync(userId, "identity", page =>
            {
                page.Details = form;
            });
        }

        var result = await _updateProfileUseCase.ExecuteAsync(userId, new UpdateProfileRequest
        {
            Username = form.Username,
            DisplayName = form.DisplayName,
            Bio = form.Bio,
            Email = form.CurrentEmail
        });

        if (!result.Success)
        {
            ModelState.AddModelError("Details.Username", result.Error ?? "Profile update failed");
            return await RenderProfileWithSectionAsync(userId, "identity", page =>
            {
                page.Details = form;
            });
        }

        await RefreshClaimsAsync(userId, form.Username, form.CurrentEmail);
        TempData["Toast.Success"] = "Profile details updated.";
        return Redirect($"{Url.Action(nameof(Index), "Profile")}#identity");
    }

    [HttpPost("ChangeEmail")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail([Bind(Prefix = "EmailChange")] ChangeEmailFormViewModel form)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Profile") });
        }

        if (!ModelState.IsValid)
        {
            return await RenderProfileWithSectionAsync(userId, "security", page =>
            {
                page.EmailChange = form;
            });
        }

        var result = await _changeEmailUseCase.ExecuteAsync(userId, new ChangeEmailRequest
        {
            CurrentPassword = form.CurrentPassword,
            NewEmail = form.NewEmail
        });

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Equals(result.Error, "Email already registered", StringComparison.OrdinalIgnoreCase)
                    ? "EmailChange.NewEmail"
                    : "EmailChange.CurrentPassword",
                result.Error ?? "Email change failed");

            return await RenderProfileWithSectionAsync(userId, "security", page =>
            {
                page.EmailChange = form;
            });
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Toast.Success"] = "Email updated. Confirm the new address before signing in again.";
        return RedirectToAction("Login", "Auth");
    }

    [HttpPost("ChangePassword")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "PasswordChange")] ChangePasswordFormViewModel form)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Profile") });
        }

        if (!ModelState.IsValid)
        {
            return await RenderProfileWithSectionAsync(userId, "security", page =>
            {
                page.PasswordChange = form;
            });
        }

        var result = await _changePasswordUseCase.ExecuteAsync(userId, new ChangePasswordRequest
        {
            CurrentPassword = form.CurrentPassword,
            NewPassword = form.NewPassword,
            ConfirmNewPassword = form.ConfirmNewPassword
        });

        if (!result.Success)
        {
            ModelState.AddModelError("PasswordChange.CurrentPassword", result.Error ?? "Password change failed");
            return await RenderProfileWithSectionAsync(userId, "security", page =>
            {
                page.PasswordChange = form;
            });
        }

        TempData["Toast.Success"] = "Password updated. A security notification email has been sent if notifications are enabled.";
        return Redirect($"{Url.Action(nameof(Index), "Profile")}#security");
    }

    [HttpPost("UpdatePreferences")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePreferences([Bind(Prefix = "Preferences")] NotificationPreferencesFormViewModel form)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Profile") });
        }

        var result = await _updateNotificationPreferencesUseCase.ExecuteAsync(userId, new UpdateNotificationPreferencesRequest
        {
            EmailNotificationsEnabled = form.EmailNotificationsEnabled
        });

        TempData[result.Success ? "Toast.Success" : "Toast.Error"] = result.Success
            ? "Notification preferences updated."
            : result.Error ?? "Failed to update preferences.";

        return Redirect($"{Url.Action(nameof(Index), "Profile")}#preferences");
    }

    [HttpPost("DeleteAccount")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount([Bind(Prefix = "DeleteAccount")] DeleteAccountFormViewModel form)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Profile") });
        }

        if (!ModelState.IsValid)
        {
            return await RenderProfileWithSectionAsync(userId, "danger", page =>
            {
                page.DeleteAccount = form;
            });
        }

        try
        {
            await _deleteAccountUseCase.ExecuteAsync(userId, form.Password);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Toast.Info"] = "Your Camagru account has been deleted.";
            return RedirectToAction("Index", "Gallery");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("DeleteAccount.Password", ex.Message);
            return await RenderProfileWithSectionAsync(userId, "danger", page =>
            {
                page.DeleteAccount = form;
            });
        }
    }

    private async Task<IActionResult> RenderProfileWithSectionAsync(int userId, string section, Action<ProfilePageViewModel> mutate)
    {
        var page = await BuildProfilePageAsync(userId);
        if (page == null)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        page.ActiveSection = section;
        mutate(page);
        return View("Index", page);
    }

    private async Task<ProfilePageViewModel?> BuildProfilePageAsync(int userId)
    {
        var result = await _getUserProfileUseCase.ExecuteAsync(userId);
        if (!result.Success || result.Data == null)
        {
            return null;
        }

        var displayName = string.IsNullOrWhiteSpace(result.Data.DisplayName)
            ? result.Data.Username
            : result.Data.DisplayName!;

        return new ProfilePageViewModel
        {
            Summary = new ProfileSummaryViewModel
            {
                UserId = result.Data.UserId,
                Username = result.Data.Username,
                DisplayName = displayName,
                Email = result.Data.Email,
                Bio = result.Data.Bio,
                EmailNotificationsEnabled = result.Data.EmailNotificationsEnabled,
                CreatedAt = result.Data.CreatedAt,
                PostCount = result.Data.PostCount
            },
            Details = new UpdateProfileFormViewModel
            {
                Username = result.Data.Username,
                DisplayName = result.Data.DisplayName,
                Bio = result.Data.Bio,
                CurrentEmail = result.Data.Email
            },
            EmailChange = new ChangeEmailFormViewModel
            {
                NewEmail = result.Data.Email
            },
            Preferences = new NotificationPreferencesFormViewModel
            {
                EmailNotificationsEnabled = result.Data.EmailNotificationsEnabled
            },
            RecentPosts = result.Data.Posts
                .OrderByDescending(post => post.CreatedAt)
                .Take(6)
                .Select(post => new ProfilePostPreviewViewModel
                {
                    PostId = post.Id,
                    ImageUrl = post.Images
                        .OrderBy(image => image.DisplayOrder)
                        .Select(image => image.FilePath)
                        .FirstOrDefault() ?? "/images/mock-gallery/orbit-01.svg",
                    Description = string.IsNullOrWhiteSpace(post.Description) ? "No caption provided yet." : post.Description,
                    CreatedLabel = post.CreatedAt.ToString("dd MMM yyyy")
                })
                .ToList()
        };
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        userId = 0;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out userId);
    }

    private async Task RefreshClaimsAsync(int userId, string username, string email)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email)
        }, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
