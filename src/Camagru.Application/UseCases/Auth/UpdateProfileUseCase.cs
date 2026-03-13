using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class UpdateProfileUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public UpdateProfileUseCase(
        IUserRepository userRepository,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
    }

    public async Task<ServiceResult> ExecuteAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return ServiceResult.Fail("User not found");

        var changes = new List<string>();

        if (request.Username != user.Username)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                return ServiceResult.Fail("Username already taken");

            user.Username = request.Username;
            changes.Add("username");
        }

        if (request.DisplayName != user.DisplayName)
        {
            user.DisplayName = request.DisplayName;
            changes.Add("display name");
        }

        if (request.Bio != user.Bio)
        {
            user.Bio = request.Bio;
            changes.Add("bio");
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
