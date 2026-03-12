using Camagru.Application.Interfaces;
using Camagru.Infrastructure.Options;
using Camagru.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Camagru.Infrastructure.DependencyInjection;

public static class SmtpExtensions
{
	public static IServiceCollection AddSmtpServices(this IServiceCollection services)
	{
		var smtpOptions = GenerateOptions();
		services.AddSingleton(smtpOptions);

		services.AddScoped<IEmailSender, EmailService>();

		return services;
	}

	private static SmtpOptions GenerateOptions()
	{
		return new SmtpOptions
		{
			Host = GetEnvVar("SMTP_HOST"),
			Port = GetEnvVar("SMTP_PORT"),
			User = Environment.GetEnvironmentVariable("SMTP_USER"),
			Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD"),
		};
	}

	private static string GetEnvVar(string name)
	{
		return Environment.GetEnvironmentVariable(name)
			?? throw new InvalidOperationException($"Missing env var: {name}");
	}
}
