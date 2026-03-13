using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class ResetPasswordUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public ResetPasswordUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
    }

    public async Task<ServiceResult> ExecuteAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByResetTokenAsync(request.Token);
        
        if (user == null)
            return ServiceResult.Fail("Invalid or expired reset token");

        // Business validation: check token expiry
        if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
            return ServiceResult.Fail("Invalid or expired reset token");

        // Delegate password hashing to infrastructure service
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // Business logic: clear reset token
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _userRepository.UpdateAsync(user);

        // Delegate email template building to infrastructure service
        var emailBody = _templateBuilder.BuildPasswordChangedNotification(user.Username);
        await _emailSender.SendAsync(user.Email, "Password Changed", emailBody);

        return ServiceResult.Ok();
    }
}
