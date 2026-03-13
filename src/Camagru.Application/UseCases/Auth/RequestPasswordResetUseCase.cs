using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class RequestPasswordResetUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public RequestPasswordResetUseCase(
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

    public async Task<ServiceResult> ExecuteAsync(RequestPasswordResetRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        // Security: Don't reveal if email exists
        if (user == null)
            return ServiceResult.Ok();

        // Delegate token generation to infrastructure service
        var resetToken = _tokenGenerator.GenerateResetToken();

        // Business logic
        user.ResetToken = resetToken;
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdateAsync(user);

        // Delegate email template building to infrastructure service
        var emailBody = _templateBuilder.BuildPasswordResetEmail(user.Username, resetToken);
        await _emailSender.SendAsync(user.Email, "Password Reset Request", emailBody);

        return ServiceResult.Ok();
    }
}
