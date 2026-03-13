using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;
using Camagru.Infrastructure.Options;
using Camagru.Infrastructure.Persistence;
using Camagru.Infrastructure.Persistence.Repositories;
using Camagru.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Camagru.Infrastructure.DependencyInjection;

public static class PersistenceExtensions
{
	public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
	{
		var persistenceOptions = GenerateOptions();
		services.AddSingleton(persistenceOptions);

		services.AddDbContext<AppDbContext>((serviceProvider, options) =>
		{
			var opts = serviceProvider.GetRequiredService<PersistenceOptions>();
			options.UseNpgsql(opts.ToConnectionString());
		});

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<ILikeRepository, LikeRepository>();
		services.AddScoped<ICommentRepository, CommentRepository>();
		services.AddScoped<IImageRepository, ImageRepository>();
		services.AddScoped<IPostRepository, PostRepository>();

		return services;
	}

	private static PersistenceOptions GenerateOptions()
	{
		return new PersistenceOptions
		{
			Host = GetEnvVar("POSTGRES_HOST"),
			Port = GetEnvVar("POSTGRES_PORT"),
			Db = GetEnvVar("POSTGRES_DB"),
			User = GetEnvVar("POSTGRES_USER"),
			Password = GetEnvVar("POSTGRES_PASSWORD"),
		};
	}

	private static string GetEnvVar(string name)
	{
		return Environment.GetEnvironmentVariable(name)
				?? throw new InvalidOperationException($"Missing env var: {name}");
	}
}
