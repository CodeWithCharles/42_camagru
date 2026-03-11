namespace Camagru.Web.Options;

public class PostgresOptions
{
    public required string Host {get; set;}
    public required string Port {get; set;}
    public required string Database {get; set;}
    public required string Username {get; set;}
    public required string Password {get; set;}
    public string ToConnectionString() =>
        $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
}