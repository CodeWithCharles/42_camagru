using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class ChangeEmailUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;
    private readonly ITokenGenerator _tokenGenerator;

    public ChangeEmailUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<ServiceResult> ExecuteAsync(int userId, ChangeEmailRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return ServiceResult.Fail("User not found");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Current password is incorrect");

        if (await _userRepository.EmailExistsAsync(request.NewEmail))
            return ServiceResult.Fail("Email already registered");

        var oldEmail = user.Email;
        user.Email = request.NewEmail;
        user.IsConfirmed = false;
        user.ConfirmationToken = _tokenGenerator.GenerateConfirmationToken();

        await _userRepository.UpdateAsync(user);

        // Send confirmation email to new address
        var confirmationLink = $"https://yourapp.com/auth/confirmemail?token={user.ConfirmationToken}";
        var emailBody = _templateBuilder.BuildConfirmationEmail(user.Username, user.ConfirmationToken);

        await _emailSender.SendAsync(request.NewEmail, "Confirm Your New Email", emailBody);

        // Notify old email of the change
        var notificationBody = $"Your email has been changed to {request.NewEmail}. If this wasn't you, please contact support.";
        await _emailSender.SendAsync(oldEmail, "Email Change Notice", notificationBody);

        return ServiceResult.Ok();
    }
}
