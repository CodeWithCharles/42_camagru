using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Posts;

public class GetPostDetailsUseCase
{
    private readonly IPostRepository _postRepository;

    public GetPostDetailsUseCase(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<ServiceResult<GalleryPostDetailsDto>> ExecuteAsync(GetPostDetailsRequest request)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(request.PostId);
        if (post == null)
        {
            return ServiceResult<GalleryPostDetailsDto>.Fail("Post not found");
        }

        var summary = ListGalleryPostsUseCase.MapSummary(post, request.ViewerUserId);
        return ServiceResult<GalleryPostDetailsDto>.Ok(new GalleryPostDetailsDto
        {
            Id = summary.Id,
            UserId = summary.UserId,
            AuthorName = summary.AuthorName,
            AuthorUsername = summary.AuthorUsername,
            Description = summary.Description,
            CreatedAt = summary.CreatedAt,
            ImageUrls = summary.ImageUrls,
            LikeCount = summary.LikeCount,
            CommentCount = summary.CommentCount,
            IsOwnedByViewer = summary.IsOwnedByViewer,
            IsLikedByViewer = summary.IsLikedByViewer,
            Comments = post.Comments
                .OrderByDescending(comment => comment.CreatedAt)
                .Select(comment => new GalleryCommentDto
                {
                    Id = comment.Id,
                    AuthorName = string.IsNullOrWhiteSpace(comment.User.DisplayName)
                        ? comment.User.Username
                        : comment.User.DisplayName!,
                    AuthorUsername = comment.User.Username,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt
                })
                .ToList()
        });
    }
}
