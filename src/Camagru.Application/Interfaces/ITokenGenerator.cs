namespace Camagru.Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateConfirmationToken();
    string GenerateResetToken();
}
