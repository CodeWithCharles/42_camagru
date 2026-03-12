using System;

namespace Camagru.Infrastructure.Options;

public class PersistenceOptions
{
	public string Host { get; set; } = string.Empty;
	public string Port { get; set; } = string.Empty;
	public string Db { get; set; } = string.Empty;
	public string User { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;

	public string ToConnectionString() =>
	$"Host={Host};Port={Port};Database={Db};Username={User};Password={Password}";
}
