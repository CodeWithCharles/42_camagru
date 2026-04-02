using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Posts;

public class ToggleLikeUseCase
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILikeRepository _likeRepository;

    public ToggleLikeUseCase(
        IPostRepository postRepository,
        IUserRepository userRepository,
        ILikeRepository likeRepository)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _likeRepository = likeRepository;
    }

    public async Task<ServiceResult<ToggleLikeResponse>> ExecuteAsync(ToggleLikeRequest request)
    {
        if (!await _postRepository.ExistsAsync(request.PostId))
        {
            return ServiceResult<ToggleLikeResponse>.Fail("Post not found");
        }

        if (!await _userRepository.ExistsAsync(request.UserId))
        {
            return ServiceResult<ToggleLikeResponse>.Fail("User not found");
        }

        var existingLike = await _likeRepository.GetByPostAndUserAsync(request.PostId, request.UserId);
        if (existingLike == null)
        {
            await _likeRepository.AddAsync(new Like
            {
                PostId = request.PostId,
                UserId = request.UserId
            });

            return ServiceResult<ToggleLikeResponse>.Ok(new ToggleLikeResponse
            {
                PostId = request.PostId,
                IsLiked = true,
                LikeCount = await _likeRepository.GetCountByPostIdAsync(request.PostId)
            });
        }

        await _likeRepository.DeleteAsync(existingLike);

        return ServiceResult<ToggleLikeResponse>.Ok(new ToggleLikeResponse
        {
            PostId = request.PostId,
            IsLiked = false,
            LikeCount = await _likeRepository.GetCountByPostIdAsync(request.PostId)
        });
    }
}
