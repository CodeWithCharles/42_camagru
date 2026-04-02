using System.ComponentModel.DataAnnotations;

namespace Camagru.Web.Models.Auth;

public class RegisterPageViewModel
{
    public RegisterFormViewModel Form { get; set; } = new();
}

public class LoginPageViewModel
{
    public LoginFormViewModel Form { get; set; } = new();
    public bool ShowConfirmationFallback { get; set; }
    public bool ConfirmationResendAvailable { get; set; }
    public string? MissingConfirmationContractName { get; set; }
    public ResendConfirmationFormViewModel ResendConfirmation { get; set; } = new();
}

public class ForgotPasswordPageViewModel
{
    public ForgotPasswordFormViewModel Form { get; set; } = new();
}

public class ResetPasswordPageViewModel
{
    public ResetPasswordFormViewModel Form { get; set; } = new();
    public bool IsTokenValid { get; set; } = true;
    public string TokenStateMessage { get; set; } = string.Empty;
}

public class RegisterConfirmationViewModel
{
    public string? Username { get; set; }
    public string? Email { get; set; }
}

public class ForgotPasswordConfirmationViewModel
{
    public string? Email { get; set; }
}

public class ConfirmEmailPageViewModel
{
    public bool IsSuccess { get; set; }
    public bool IsAlreadyConfirmed { get; set; }
    public bool CanResendConfirmation { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string PrimaryActionText { get; set; } = string.Empty;
    public string PrimaryActionUrl { get; set; } = string.Empty;
    public string? SecondaryActionText { get; set; }
    public string? SecondaryActionUrl { get; set; }
    public string? MissingContractName { get; set; }
    public ResendConfirmationFormViewModel ResendConfirmation { get; set; } = new();
}

public class ResendConfirmationConfirmationViewModel
{
    public string? Email { get; set; }
}

public class RegisterFormViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginFormViewModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class ForgotPasswordFormViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;
}

public class ResendConfirmationFormViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public string Origin { get; set; } = "login";
}

public class ResetPasswordFormViewModel
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
