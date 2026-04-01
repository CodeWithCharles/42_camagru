using Camagru.Web.Models.Shared;

namespace Camagru.Web.Models.Gallery;

public class GalleryIndexViewModel
{
    public IReadOnlyList<GalleryPostCardViewModel> Posts { get; set; } = [];
    public GalleryPostModalViewModel? ActivePost { get; set; }
    public PaginationViewModel Pagination { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public bool IsAuthenticated { get; set; }
    public string LoginUrl { get; set; } = string.Empty;
}

public class GalleryPostCardViewModel
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorHandle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;
    public string CreatedLabel { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ImageCount { get; set; }
    public string OpenUrl { get; set; } = string.Empty;
    public bool IsOwnedByCurrentUser { get; set; }
}

public class GalleryPostModalViewModel
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorHandle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedLabel { get; set; } = string.Empty;
    public IReadOnlyList<string> ImageUrls { get; set; } = [];
    public IReadOnlyList<GalleryCommentViewModel> Comments { get; set; } = [];
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsAuthenticated { get; set; }
    public bool IsOwnedByCurrentUser { get; set; }
    public string CloseUrl { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public string LoginUrl { get; set; } = string.Empty;
    public string LikeActionUrl { get; set; } = string.Empty;
    public string CommentActionUrl { get; set; } = string.Empty;
    public string DeleteActionUrl { get; set; } = string.Empty;
}

public class GalleryCommentViewModel
{
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorHandle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string CreatedLabel { get; set; } = string.Empty;
}

public class EmptyGalleryViewModel
{
    public bool IsAuthenticated { get; set; }
    public string PrimaryActionText { get; set; } = string.Empty;
    public string PrimaryActionUrl { get; set; } = string.Empty;
    public string SecondaryActionText { get; set; } = string.Empty;
    public string SecondaryActionUrl { get; set; } = string.Empty;
}

public class GalleryInteractionInputModel
{
    public int PostId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
