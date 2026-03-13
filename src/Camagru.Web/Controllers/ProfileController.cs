using System.Security.Claims;
using Camagru.Application.Contracts.Auth;
using Camagru.Application.UseCases.Auth;
using Camagru.Web.Models;
using Microsoft.AspNetCore.Authentication;
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

    // GET /Profile or /Profile/{userId}
    [HttpGet]
    [HttpGet("{userId}")]
    public async Task<IActionResult> Index(int? userId)
    {
        // If no userId provided, use the logged-in user
        if (!userId.HasValue)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return RedirectToAction("Login", "Auth");

            if (!int.TryParse(userIdClaim.Value, out var parsedUserId))
                return RedirectToAction("Login", "Auth");

            userId = parsedUserId;
        }

        var result = await _getUserProfileUseCase.ExecuteAsync(userId.Value);
        if (!result.Success)
            return NotFound("User not found");

        // Check if this is the logged-in user's profile
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        ViewData["IsOwnProfile"] = currentUserIdClaim?.Value == userId.ToString();

        return View(result.Data);
    }

    // GET /Profile/Edit
    [HttpGet("Edit")]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return RedirectToAction("Login", "Auth");

        var result = await _getUserProfileUseCase.ExecuteAsync(userId);
        if (!result.Success)
            return NotFound("User not found");

        var editViewModel = new EditProfileViewModel
        {
            Username = result.Data!.Username,
            DisplayName = result.Data!.DisplayName,
            Bio = result.Data!.Bio,
            Email = result.Data!.Email
        };

        return View(editViewModel);
    }

    // POST /Profile/Edit
    [HttpPost("Edit")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
            return View(model);

        var updateRequest = new UpdateProfileRequest
        {
            Username = model.Username,
            DisplayName = model.DisplayName,
            Bio = model.Bio,
            Email = model.Email
        };

        var result = await _updateProfileUseCase.ExecuteAsync(userId, updateRequest);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Profile update failed");
            return View(model);
        }

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET /Profile/Settings
    [HttpGet("Settings")]
    [Authorize]
    public async Task<IActionResult> Settings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return RedirectToAction("Login", "Auth");

        var result = await _getUserProfileUseCase.ExecuteAsync(userId);
        if (!result.Success)
            return NotFound("User not found");

        var settingsViewModel = new SettingsViewModel
        {
            Email = result.Data!.Email,
            EmailNotificationsEnabled = result.Data!.EmailNotificationsEnabled
        };

        return View(settingsViewModel);
    }

    // POST /Profile/Settings
    [HttpPost("Settings")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SettingsViewModel model)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return RedirectToAction("Login", "Auth");

        var request = new UpdateNotificationPreferencesRequest
        {
            EmailNotificationsEnabled = model.EmailNotificationsEnabled
        };

        var result = await _updateNotificationPreferencesUseCase.ExecuteAsync(userId, request);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Error ?? "Failed to update settings";
            return RedirectToAction(nameof(Settings));
        }

        TempData["SuccessMessage"] = "Settings updated successfully!";
        return RedirectToAction(nameof(Settings));
    }

    // POST /Profile/ChangePassword (AJAX)
    [HttpPost("ChangePassword")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Json(new { success = false, error = "Unauthorized" });

        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid request data" });

        var result = await _changePasswordUseCase.ExecuteAsync(userId, request);
        if (!result.Success)
            return Json(new { success = false, error = result.Error });

        return Json(new { success = true, message = "Password changed successfully! Check your email for confirmation." });
    }

    // POST /Profile/ChangeEmail (AJAX)
    [HttpPost("ChangeEmail")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Json(new { success = false, error = "Unauthorized" });

        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid request data" });

        var result = await _changeEmailUseCase.ExecuteAsync(userId, request);
        if (!result.Success)
            return Json(new { success = false, error = result.Error });

        return Json(new { success = true, message = "Email changed successfully! Check your new email for a confirmation link." });
    }

    // POST /Profile/DeleteAccount (AJAX)
    [HttpPost("DeleteAccount")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount([FromBody] dynamic request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Json(new { success = false, error = "Unauthorized" });

        try
        {
            var password = request.password;
            if (string.IsNullOrEmpty(password))
                return Json(new { success = false, error = "Password is required" });

            await _deleteAccountUseCase.ExecuteAsync(userId, password);
            await HttpContext.SignOutAsync();
            return Json(new { success = true, message = "Account deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}
