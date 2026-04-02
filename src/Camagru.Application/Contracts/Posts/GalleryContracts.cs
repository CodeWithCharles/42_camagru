namespace Camagru.Application.Contracts.Posts;

public class ListGalleryPostsRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int? ViewerUserId { get; set; }
}

public class ListGalleryPostsResponse
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<GalleryPostSummaryDto> Posts { get; set; } = [];
}

public class GetPostDetailsRequest
{
    public int PostId { get; set; }
    public int? ViewerUserId { get; set; }
}

public class GalleryPostSummaryDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<string> ImageUrls { get; set; } = [];
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsOwnedByViewer { get; set; }
    public bool IsLikedByViewer { get; set; }
}

public class GalleryPostDetailsDto : GalleryPostSummaryDto
{
    public IReadOnlyList<GalleryCommentDto> Comments { get; set; } = [];
}

public class GalleryCommentDto
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUsername { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ToggleLikeRequest
{
    public int PostId { get; set; }
    public int UserId { get; set; }
}

public class ToggleLikeResponse
{
    public int PostId { get; set; }
    public bool IsLiked { get; set; }
    public int LikeCount { get; set; }
}

public class AddCommentRequest
{
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class AddCommentResponse
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    public int CommentCount { get; set; }
    public string? WarningMessage { get; set; }
}

public class DeletePostRequest
{
    public int PostId { get; set; }
    public int RequestingUserId { get; set; }
}
