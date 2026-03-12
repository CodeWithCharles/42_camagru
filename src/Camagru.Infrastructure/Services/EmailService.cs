using Camagru.Application.Interfaces;
using Camagru.Infrastructure.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace Camagru.Infrastructure.Services;

public class EmailService : IEmailSender
{
	private readonly SmtpOptions _smtpOptions;

	public EmailService(SmtpOptions smtpOptions)
	{
		_smtpOptions = smtpOptions;
	}

	public async Task SendAsync(string to, string subject, string body)
	{
		var message = new MimeMessage();
		message.From.Add(new MailboxAddress("Camagru", _smtpOptions.User ?? "noreply@camagru.local"));
		message.To.Add(MailboxAddress.Parse(to));
		message.Subject = subject;

		var bodyBuilder = new BodyBuilder
		{
			HtmlBody = body
		};
		message.Body = bodyBuilder.ToMessageBody();

		using var client = new SmtpClient();
		await client.ConnectAsync(_smtpOptions.Host, int.Parse(_smtpOptions.Port), false);

		// Only authenticate if credentials are provided
		if (!string.IsNullOrWhiteSpace(_smtpOptions.User) && !string.IsNullOrWhiteSpace(_smtpOptions.Password))
		{
			await client.AuthenticateAsync(_smtpOptions.User, _smtpOptions.Password);
		}

		await client.SendAsync(message);
		await client.DisconnectAsync(true);
	}
}
