namespace Camagru.Infrastructure.Options;

public class SmtpOptions
{
	public required string Host { get; set; }
	public required string Port { get; set; }
	public string? User { get; set; }
	public string? Password { get; set; }
}
