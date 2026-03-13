using System.Security.Claims;
using Camagru.Application.Contracts.Auth;
using Camagru.Application.UseCases.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly RegisterUseCase _registerUseCase;
    private readonly ConfirmEmailUseCase _confirmEmailUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly RequestPasswordResetUseCase _requestPasswordResetUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;

    public AuthController(
        RegisterUseCase registerUseCase,
        ConfirmEmailUseCase confirmEmailUseCase,
        LoginUseCase loginUseCase,
        RequestPasswordResetUseCase requestPasswordResetUseCase,
        ResetPasswordUseCase resetPasswordUseCase)
    {
        _registerUseCase = registerUseCase;
        _confirmEmailUseCase = confirmEmailUseCase;
        _loginUseCase = loginUseCase;
        _requestPasswordResetUseCase = requestPasswordResetUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _registerUseCase.ExecuteAsync(request);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed");
            return View(request);
        }

        return RedirectToAction(nameof(RegisterConfirmation));
    }

    [HttpGet]
    public IActionResult RegisterConfirmation()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("Error", "Invalid confirmation token");
        }

        var result = await _confirmEmailUseCase.ExecuteAsync(new ConfirmEmailRequest { Token = token });

        if (!result.Success)
        {
            return View("Error", result.Error ?? "Email confirmation failed");
        }

        TempData["SuccessMessage"] = "Email confirmed successfully! Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _loginUseCase.ExecuteAsync(request);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed");
            return View(request);
        }

        // Build claims from LoginResponse
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
            new Claim(ClaimTypes.Name, result.Data.Username),
            new Claim(ClaimTypes.Email, result.Data.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new RequestPasswordResetRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(RequestPasswordResetRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        // Always call the use case regardless of whether email exists
        await _requestPasswordResetUseCase.ExecuteAsync(request);

        // Always redirect to confirmation (security best practice)
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("Error", "Invalid reset token");
        }

        return View(new ResetPasswordRequest { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _resetPasswordUseCase.ExecuteAsync(request);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Password reset failed");
            return View(request);
        }

        TempData["SuccessMessage"] = "Password reset successfully! Please log in with your new password.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Error(string message)
    {
        ViewBag.ErrorMessage = message;
        return View();
    }
}
