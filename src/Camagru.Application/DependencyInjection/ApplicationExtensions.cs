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
        services.AddScoped<ResendConfirmationEmailUseCase>();
        services.AddScoped<GetAvailableOverlaysUseCase>();
        services.AddScoped<ListGalleryPostsUseCase>();
        services.AddScoped<GetPostDetailsUseCase>();
        services.AddScoped<ToggleLikeUseCase>();
        services.AddScoped<AddCommentUseCase>();
        services.AddScoped<DeletePostUseCase>();
        services.AddScoped<PublishMontageUseCase>();

        return services;
    }
}
