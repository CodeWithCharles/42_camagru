using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class DeleteAccountUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public DeleteAccountUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task ExecuteAsync(int userId, string password)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Invalid password.");

        // Delete user - cascade delete will handle posts, comments, likes
        await _userRepository.DeleteAsync(user);
    }
}
