using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class ConfirmEmailUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public ConfirmEmailUseCase(
        IUserRepository userRepository,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
    }

    public async Task<ServiceResult> ExecuteAsync(ConfirmEmailRequest request)
    {
        var user = await _userRepository.GetByConfirmationTokenAsync(request.Token);
        
        if (user == null)
            return ServiceResult.Fail("Invalid confirmation token");

        if (user.IsConfirmed)
            return ServiceResult.Fail("Email already confirmed");

        // Business logic
        user.IsConfirmed = true;
        user.ConfirmationToken = null;

        await _userRepository.UpdateAsync(user);

        // Delegate email template building to infrastructure service
        var emailBody = _templateBuilder.BuildWelcomeEmail(user.Username);
        await _emailSender.SendAsync(user.Email, "Welcome to Camagru!", emailBody);

        return ServiceResult.Ok();
    }
}
