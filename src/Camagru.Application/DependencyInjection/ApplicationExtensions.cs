using Camagru.Application.UseCases.Auth;
using Camagru.Application.UseCases.Posts;
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
        services.AddScoped<GetUserProfileUseCase>();
        services.AddScoped<ChangePasswordUseCase>();
        services.AddScoped<ChangeEmailUseCase>();
        services.AddScoped<DeleteAccountUseCase>();
        services.AddScoped<UpdateNotificationPreferencesUseCase>();
        services.AddScoped<GetAvailableOverlaysUseCase>();

        return services;
    }
}
