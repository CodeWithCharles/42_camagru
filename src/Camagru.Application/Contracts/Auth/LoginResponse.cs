namespace Camagru.Application.Contracts.Auth;

public class LoginResponse
{
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
