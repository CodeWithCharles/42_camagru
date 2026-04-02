using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Posts;

public class ListGalleryPostsUseCase
{
    private readonly IPostRepository _postRepository;

    public ListGalleryPostsUseCase(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<ServiceResult<ListGalleryPostsResponse>> ExecuteAsync(ListGalleryPostsRequest request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 6 : request.PageSize;
        var (posts, totalCount) = await _postRepository.GetPagedGalleryAsync(page, pageSize);

        return ServiceResult<ListGalleryPostsResponse>.Ok(new ListGalleryPostsResponse
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Posts = posts.Select(post => MapSummary(post, request.ViewerUserId)).ToList()
        });
    }

    internal static GalleryPostSummaryDto MapSummary(Post post, int? viewerUserId)
    {
        var authorName = string.IsNullOrWhiteSpace(post.User.DisplayName) ? post.User.Username : post.User.DisplayName!;

        return new GalleryPostSummaryDto
        {
            Id = post.Id,
            UserId = post.UserId,
            AuthorName = authorName,
            AuthorUsername = post.User.Username,
            Description = string.IsNullOrWhiteSpace(post.Description) ? "No caption provided." : post.Description,
            CreatedAt = post.CreatedAt,
            ImageUrls = post.Images
                .OrderBy(image => image.DisplayOrder)
                .Select(image => image.FilePath)
                .ToList(),
            LikeCount = post.Likes.Count,
            CommentCount = post.Comments.Count,
            IsOwnedByViewer = viewerUserId.HasValue && post.UserId == viewerUserId.Value,
            IsLikedByViewer = viewerUserId.HasValue && post.Likes.Any(like => like.UserId == viewerUserId.Value)
        };
    }
}
