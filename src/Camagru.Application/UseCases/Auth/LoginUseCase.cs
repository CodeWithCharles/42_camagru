using Camagru.Application.Contracts.Auth;
using Camagru.Application.Contracts.Common;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ServiceResult<LoginResponse>> ExecuteAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        
        if (user == null)
            return ServiceResult<LoginResponse>.Fail("Invalid username or password");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return ServiceResult<LoginResponse>.Fail("Invalid username or password");

        if (!user.IsConfirmed)
            return ServiceResult<LoginResponse>.Fail("Please confirm your email before logging in");

        return ServiceResult<LoginResponse>.Ok(new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email
        });
    }
}
