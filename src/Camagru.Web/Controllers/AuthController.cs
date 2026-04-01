using System.Security.Claims;
using Camagru.Application.Contracts.Auth;
using Camagru.Application.UseCases.Auth;
using Camagru.Domain.Interfaces;
using Camagru.Web.Models.Auth;
using Camagru.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private const string RegisterEmailTempDataKey = "Auth.RegisterEmail";
    private const string RegisterUsernameTempDataKey = "Auth.RegisterUsername";
    private const string ForgotPasswordEmailTempDataKey = "Auth.ForgotPasswordEmail";
    private readonly RegisterUseCase _registerUseCase;
    private readonly ConfirmEmailUseCase _confirmEmailUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly RequestPasswordResetUseCase _requestPasswordResetUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;
    private readonly IUserRepository _userRepository;
    private readonly UiFeatureFlags _uiFeatureFlags;

    public AuthController(
        RegisterUseCase registerUseCase,
        ConfirmEmailUseCase confirmEmailUseCase,
        LoginUseCase loginUseCase,
        RequestPasswordResetUseCase requestPasswordResetUseCase,
        ResetPasswordUseCase resetPasswordUseCase,
        IUserRepository userRepository,
        UiFeatureFlags uiFeatureFlags)
    {
        _registerUseCase = registerUseCase;
        _confirmEmailUseCase = confirmEmailUseCase;
        _loginUseCase = loginUseCase;
        _requestPasswordResetUseCase = requestPasswordResetUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _userRepository = userRepository;
        _uiFeatureFlags = uiFeatureFlags;
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        return View(new RegisterPageViewModel());
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _registerUseCase.ExecuteAsync(new RegisterRequest
        {
            Username = model.Form.Username,
            Email = model.Form.Email,
            Password = model.Form.Password,
            ConfirmPassword = model.Form.ConfirmPassword
        });

        if (!result.Success)
        {
            MapRegisterError(model, result.Error);
            return View(model);
        }

        TempData[RegisterEmailTempDataKey] = model.Form.Email;
        TempData[RegisterUsernameTempDataKey] = model.Form.Username;
        TempData["Toast.Success"] = "Account created. Confirm your email to unlock the editor and profile.";
        return RedirectToAction(nameof(RegisterConfirmation));
    }

    [HttpGet("RegisterConfirmation")]
    public IActionResult RegisterConfirmation()
    {
        return View(new RegisterConfirmationViewModel
        {
            Email = TempData.Peek(RegisterEmailTempDataKey) as string,
            Username = TempData.Peek(RegisterUsernameTempDataKey) as string
        });
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View(BuildFailedConfirmationViewModel("The confirmation link is missing or malformed."));
        }

        var result = await _confirmEmailUseCase.ExecuteAsync(new ConfirmEmailRequest { Token = token });
        if (result.Success)
        {
            return View(new ConfirmEmailPageViewModel
            {
                IsSuccess = true,
                Title = "Email Confirmed",
                Headline = "Connection uplink established",
                Message = "Your Camagru account is confirmed. You can sign in and start building montages now.",
                PrimaryActionText = "Go to login",
                PrimaryActionUrl = Url.Action(nameof(Login), "Auth") ?? "/Auth/Login",
                SecondaryActionText = "Open gallery",
                SecondaryActionUrl = Url.Action("Index", "Gallery") ?? "/Gallery"
            });
        }

        if (string.Equals(result.Error, "Email already confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return View(new ConfirmEmailPageViewModel
            {
                IsAlreadyConfirmed = true,
                Title = "Email Already Confirmed",
                Headline = "This uplink was already verified",
                Message = "That confirmation link has already been used. Your account is ready to sign in.",
                PrimaryActionText = "Go to login",
                PrimaryActionUrl = Url.Action(nameof(Login), "Auth") ?? "/Auth/Login",
                SecondaryActionText = "Open gallery",
                SecondaryActionUrl = Url.Action("Index", "Gallery") ?? "/Gallery"
            });
        }

        return View(BuildFailedConfirmationViewModel(result.Error ?? "Email confirmation failed."));
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginPageViewModel
        {
            Form = new LoginFormViewModel
            {
                ReturnUrl = returnUrl
            }
        });
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _loginUseCase.ExecuteAsync(new LoginRequest
        {
            Username = model.Form.Username,
            Password = model.Form.Password,
            RememberMe = model.Form.RememberMe
        });

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Please confirm your email before logging in", StringComparison.OrdinalIgnoreCase))
            {
                model.ShowConfirmationFallback = true;
                model.ConfirmationResendAvailable = _uiFeatureFlags.EnableConfirmationResend;
                model.MissingConfirmationContractName = _uiFeatureFlags.EnableConfirmationResend ? null : "ResendConfirmationEmailUseCase";
                ModelState.AddModelError("Form.Username", result.Error ?? "Please confirm your email before logging in");
            }
            else
            {
                ModelState.AddModelError("Form.Username", result.Error ?? "Login failed");
            }

            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
            new(ClaimTypes.Name, result.Data.Username),
            new(ClaimTypes.Email, result.Data.Email)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.Form.RememberMe,
            ExpiresUtc = model.Form.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(1)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["Toast.Success"] = $"Welcome back, {result.Data.Username}.";
        return RedirectToLocal(model.Form.ReturnUrl, fallbackAction: "Index", fallbackController: "Gallery");
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Toast.Info"] = "You have been logged out.";
        return RedirectToAction("Index", "Gallery");
    }

    [HttpGet("ForgotPassword")]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordPageViewModel());
    }

    [HttpPost("ForgotPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _requestPasswordResetUseCase.ExecuteAsync(new RequestPasswordResetRequest
        {
            Email = model.Form.Email
        });

        TempData[ForgotPasswordEmailTempDataKey] = model.Form.Email;
        TempData["Toast.Info"] = "If the address exists in Camagru, a reset link is on its way.";
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet("ForgotPasswordConfirmation")]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View(new ForgotPasswordConfirmationViewModel
        {
            Email = TempData.Peek(ForgotPasswordEmailTempDataKey) as string
        });
    }

    [HttpGet("ResetPassword")]
    public async Task<IActionResult> ResetPassword(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View(new ResetPasswordPageViewModel
            {
                IsTokenValid = false,
                TokenStateMessage = "The reset link is incomplete or missing."
            });
        }

        var user = await _userRepository.GetByResetTokenAsync(token);
        return View(new ResetPasswordPageViewModel
        {
            IsTokenValid = user != null,
            TokenStateMessage = user == null ? "This reset link is invalid or expired." : string.Empty,
            Form = new ResetPasswordFormViewModel
            {
                Token = token
            }
        });
    }

    [HttpPost("ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _resetPasswordUseCase.ExecuteAsync(new ResetPasswordRequest
        {
            Token = model.Form.Token,
            NewPassword = model.Form.NewPassword,
            ConfirmNewPassword = model.Form.ConfirmNewPassword
        });

        if (!result.Success)
        {
            model.IsTokenValid = !string.Equals(result.Error, "Invalid or expired reset token", StringComparison.OrdinalIgnoreCase);
            model.TokenStateMessage = model.IsTokenValid ? string.Empty : (result.Error ?? "Invalid or expired reset token");
            if (model.IsTokenValid)
            {
                ModelState.AddModelError("Form.NewPassword", result.Error ?? "Password reset failed");
            }

            return View(model);
        }

        TempData["Toast.Success"] = "Password reset complete. Sign in with your new credentials.";
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl, string fallbackAction, string fallbackController)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(fallbackAction, fallbackController);
    }

    private ConfirmEmailPageViewModel BuildFailedConfirmationViewModel(string message)
    {
        return new ConfirmEmailPageViewModel
        {
            Title = "Confirmation Failed",
            Headline = "The uplink could not be verified",
            Message = message,
            PrimaryActionText = "Try login",
            PrimaryActionUrl = Url.Action(nameof(Login), "Auth") ?? "/Auth/Login",
            SecondaryActionText = "Resend confirmation",
            SecondaryActionUrl = Url.Action("FeatureNotReady", "Errors", new
            {
                feature = "confirmation resend",
                missingContract = "ResendConfirmationEmailUseCase",
                returnUrl = Url.Action(nameof(Login), "Auth")
            }) ?? "/Errors/FeatureNotReady",
            CanResendConfirmation = _uiFeatureFlags.EnableConfirmationResend,
            MissingContractName = _uiFeatureFlags.EnableConfirmationResend ? null : "ResendConfirmationEmailUseCase"
        };
    }

    private void MapRegisterError(RegisterPageViewModel model, string? error)
    {
        if (string.Equals(error, "Email already registered", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Form.Email", error ?? "Email already registered");
            return;
        }

        if (string.Equals(error, "Username already taken", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Form.Username", error ?? "Username already taken");
            return;
        }

        ModelState.AddModelError("Form.Username", error ?? "Registration failed");
    }
}
