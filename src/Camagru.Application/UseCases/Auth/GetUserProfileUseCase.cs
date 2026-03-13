using Camagru.Application.Contracts.Common;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Auth;

public class GetUserProfileUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;

    public GetUserProfileUseCase(IUserRepository userRepository, IPostRepository postRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
    }

    public async Task<ServiceResult<UserProfileData>> ExecuteAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
            return ServiceResult<UserProfileData>.Fail("User not found");

        var posts = await _postRepository.GetByUserIdAsync(userId);

        var profileData = new UserProfileData
        {
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            AvatarImageId = user.AvatarImageId,
            Email = user.Email,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled,
            CreatedAt = user.CreatedAt,
            PostCount = posts.Count(),
            Posts = posts.OrderByDescending(p => p.Id).ToList()
        };

        return ServiceResult<UserProfileData>.Ok(profileData);
    }
}

public class UserProfileData
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public int? AvatarImageId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PostCount { get; set; }
    public List<Post> Posts { get; set; } = new();
}
