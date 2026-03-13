using System.Security.Cryptography;
using Camagru.Application.Interfaces;

namespace Camagru.Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    public string GenerateConfirmationToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public string GenerateResetToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
