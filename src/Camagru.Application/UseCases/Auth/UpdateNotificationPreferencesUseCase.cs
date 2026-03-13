using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class UpdateNotificationPreferencesUseCase
{
    private readonly IUserRepository _userRepository;

    public UpdateNotificationPreferencesUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ServiceResult> ExecuteAsync(int userId, UpdateNotificationPreferencesRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return ServiceResult.Fail("User not found");

        user.EmailNotificationsEnabled = request.EmailNotificationsEnabled;
        await _userRepository.UpdateAsync(user);

        return ServiceResult.Ok();
    }
}
