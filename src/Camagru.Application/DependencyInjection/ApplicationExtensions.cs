using Camagru.Application.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Camagru.Application.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUseCase>();
        services.AddScoped<ConfirmEmailUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<RequestPasswordResetUseCase>();
        services.AddScoped<ResetPasswordUseCase>();
        services.AddScoped<UpdateProfileUseCase>();

        return services;
    }
}
