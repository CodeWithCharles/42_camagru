using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class ChangePasswordUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public ChangePasswordUseCase(
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

    public async Task<ServiceResult> ExecuteAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return ServiceResult.Fail("User not found");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Current password is incorrect");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        // Send confirmation email with security context
        if (user.EmailNotificationsEnabled)
        {
            var emailBody = _templateBuilder.BuildPasswordChangedNotification(user.Username);
            await _emailSender.SendAsync(user.Email, "Password Changed", emailBody);
        }

        return ServiceResult.Ok();
    }
}
