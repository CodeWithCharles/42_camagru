using Camagru.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Camagru.Infrastructure.Services;

public class EmailTemplateBuilder : IEmailTemplateBuilder
{
    private readonly string _baseUrl;

    public EmailTemplateBuilder(IConfiguration configuration)
    {
        _baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") 
            ?? configuration["App:BaseUrl"] 
            ?? "http://localhost";
    }

    public string BuildConfirmationEmail(string username, string confirmationToken)
    {
        var confirmUrl = $"{_baseUrl}/auth/confirmemail?token={Uri.EscapeDataString(confirmationToken)}";
        
        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 24px; 
            background-color: #007bff; 
            color: white; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0;
        }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Welcome to Camagru, {username}!</h1>
        <p>Thank you for registering. Please confirm your email address to activate your account.</p>
        <a href='{confirmUrl}' class='button'>Confirm Email Address</a>
        <p>Or copy and paste this link into your browser:</p>
        <p style='word-break: break-all;'>{confirmUrl}</p>
        <div class='footer'>
            <p>If you didn't create this account, please ignore this email.</p>
            <p>&copy; 2026 Camagru. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildPasswordResetEmail(string username, string resetToken)
    {
        var resetUrl = $"{_baseUrl}/auth/resetpassword?token={Uri.EscapeDataString(resetToken)}";
        
        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 24px; 
            background-color: #dc3545; 
            color: white; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0;
        }}
        .warning {{ background-color: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Password Reset Request</h1>
        <p>Hello {username},</p>
        <p>We received a request to reset your password. Click the button below to create a new password:</p>
        <a href='{resetUrl}' class='button'>Reset Password</a>
        <p>Or copy and paste this link into your browser:</p>
        <p style='word-break: break-all;'>{resetUrl}</p>
        <div class='warning'>
            <strong>Security Notice:</strong> This link will expire in 1 hour for security reasons.
        </div>
        <div class='footer'>
            <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
            <p>&copy; 2026 Camagru. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildPasswordChangedNotification(string username)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .alert {{ background-color: #d4edda; padding: 15px; border-left: 4px solid #28a745; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Password Changed Successfully</h1>
        <p>Hello {username},</p>
        <div class='alert'>
            <strong>Confirmed:</strong> Your password has been changed successfully.
        </div>
        <p>If you did not make this change, please contact us immediately.</p>
        <div class='footer'>
            <p>&copy; 2026 Camagru. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildProfileUpdatedNotification(string username, string changes)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .info {{ background-color: #d1ecf1; padding: 15px; border-left: 4px solid #17a2b8; margin: 20px 0; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Profile Updated</h1>
        <p>Hello {username},</p>
        <div class='info'>
            <strong>Info:</strong> Your profile has been updated.
            <br><strong>Changes:</strong> {changes}
        </div>
        <p>If you did not make these changes, please contact us immediately.</p>
        <div class='footer'>
            <p>&copy; 2026 Camagru. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string BuildWelcomeEmail(string username)
    {
        return $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 24px; 
            background-color: #28a745; 
            color: white; 
            text-decoration: none; 
            border-radius: 4px; 
            margin: 20px 0;
        }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>Welcome to Camagru, {username}!</h1>
        <p>Your account is now active. Start creating and sharing amazing photo compositions!</p>
        <a href='{_baseUrl}/gallery' class='button'>Explore Gallery</a>
        <div class='footer'>
            <p>&copy; 2026 Camagru. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
