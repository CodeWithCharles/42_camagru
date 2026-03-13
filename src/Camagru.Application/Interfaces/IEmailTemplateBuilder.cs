namespace Camagru.Application.Interfaces;

public interface IEmailTemplateBuilder
{
    string BuildConfirmationEmail(string username, string confirmationToken);
    string BuildPasswordResetEmail(string username, string resetToken);
    string BuildPasswordChangedNotification(string username);
    string BuildProfileUpdatedNotification(string username, string changes);
    string BuildWelcomeEmail(string username);
}
