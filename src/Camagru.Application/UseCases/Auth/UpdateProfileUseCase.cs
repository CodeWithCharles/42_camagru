using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class UpdateProfileUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public UpdateProfileUseCase(
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

    public async Task<ServiceResult> ExecuteAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return ServiceResult.Fail("User not found");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Current password is incorrect");

        var changes = new List<string>();

        if (request.Username != user.Username)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                return ServiceResult.Fail("Username already taken");

            user.Username = request.Username;
            changes.Add("username");
        }

        if (request.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(request.Email))
                return ServiceResult.Fail("Email already registered");

            user.Email = request.Email;
            changes.Add("email");
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            // Delegate password hashing to infrastructure service
            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            changes.Add("password");
        }

        await _userRepository.UpdateAsync(user);

        if (user.EmailNotificationsEnabled && changes.Any())
        {
            var emailBody = _templateBuilder.BuildProfileUpdatedNotification(
                user.Username, 
                string.Join(", ", changes)
            );

            await _emailSender.SendAsync(user.Email, "Profile Updated", emailBody);
        }

        return ServiceResult.Ok();
    }
}
