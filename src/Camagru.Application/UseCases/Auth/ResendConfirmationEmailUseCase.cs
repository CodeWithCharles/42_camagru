using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class ResendConfirmationEmailUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public ResendConfirmationEmailUseCase(
        IUserRepository userRepository,
        ITokenGenerator tokenGenerator,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
    }

    public async Task<ServiceResult> ExecuteAsync(ResendConfirmationEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || user.IsConfirmed)
        {
            return ServiceResult.Ok();
        }

        user.ConfirmationToken = _tokenGenerator.GenerateConfirmationToken();
        await _userRepository.UpdateAsync(user);

        var emailBody = _templateBuilder.BuildConfirmationEmail(user.Username, user.ConfirmationToken);
        await _emailSender.SendAsync(user.Email, "Confirm your Camagru account", emailBody);

        return ServiceResult.Ok();
    }
}
