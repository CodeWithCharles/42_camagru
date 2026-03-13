using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class RegisterUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateBuilder _templateBuilder;

    public RegisterUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IEmailSender emailSender,
        IEmailTemplateBuilder templateBuilder)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _emailSender = emailSender;
        _templateBuilder = templateBuilder;
    }

    public async Task<ServiceResult> ExecuteAsync(RegisterRequest request)
    {
        // Business validation
        if (await _userRepository.EmailExistsAsync(request.Email))
            return ServiceResult.Fail("Email already registered");

        if (await _userRepository.UsernameExistsAsync(request.Username))
            return ServiceResult.Fail("Username already taken");

        // Delegate technical concerns to infrastructure services
        var passwordHash = _passwordHasher.Hash(request.Password);
        var confirmationToken = _tokenGenerator.GenerateConfirmationToken();

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            ConfirmationToken = confirmationToken,
            IsConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        // Delegate email template building to infrastructure service
        var emailBody = _templateBuilder.BuildConfirmationEmail(user.Username, confirmationToken);
        await _emailSender.SendAsync(user.Email, "Confirm your Camagru account", emailBody);

        return ServiceResult.Ok();
    }
}
