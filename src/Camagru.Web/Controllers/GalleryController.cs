using System.Security.Claims;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Camagru.Web.Models.Gallery;
using Camagru.Web.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class GalleryController : Controller
{
    private const int PageSize = 6;
    private readonly IPostRepository _postRepository;

    public GalleryController(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1, int? postId = null)
    {
        if (page < 1)
        {
            return RedirectToAction(nameof(Index), new { page = 1 });
        }

        var (posts, totalCount) = await _postRepository.GetPagedGalleryAsync(page, PageSize);
        if (totalCount == 0)
        {
            return View("Empty", new EmptyGalleryViewModel
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                PrimaryActionText = User.Identity?.IsAuthenticated ?? false ? "Open editor" : "Create an account",
                PrimaryActionUrl = User.Identity?.IsAuthenticated ?? false
                    ? Url.Action("Index", "Editor") ?? "/Editor"
                    : Url.Action("Register", "Auth") ?? "/Auth/Register",
                SecondaryActionText = User.Identity?.IsAuthenticated ?? false ? "Review your profile" : "Sign in",
                SecondaryActionUrl = User.Identity?.IsAuthenticated ?? false
                    ? Url.Action("Index", "Profile") ?? "/Profile"
                    : Url.Action("Login", "Auth") ?? "/Auth/Login"
            });
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (page > totalPages)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        var currentUserId = TryGetCurrentUserId();
        var selectedPost = postId.HasValue ? await _postRepository.GetByIdWithDetailsAsync(postId.Value) : null;
        if (postId.HasValue && selectedPost == null)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        var model = new GalleryIndexViewModel
        {
            CurrentPage = page,
            TotalItems = totalCount,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            LoginUrl = Url.Action("Login", "Auth", new
            {
                returnUrl = Url.Action(nameof(Index), new { page, postId })
            }) ?? "/Auth/Login",
            Posts = posts.Select(post => MapCard(post, page, currentUserId)).ToList(),
            ActivePost = selectedPost == null ? null : MapModal(selectedPost, page, currentUserId),
            Pagination = BuildPagination(page, totalPages)
        };

        return View(model);
    }

    [HttpPost("Like")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(GalleryInteractionInputModel input)
    {
        if (!await _postRepository.ExistsAsync(input.PostId))
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        TempData["Toast.Info"] = "Likes are visible in the UI, but persistence is not wired yet.";
        return RedirectToLocal(input.ReturnUrl);
    }

    [HttpPost("Comment")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(GalleryInteractionInputModel input)
    {
        if (!await _postRepository.ExistsAsync(input.PostId))
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        TempData["Toast.Info"] = string.IsNullOrWhiteSpace(input.Comment)
            ? "Add a message before sending a placeholder comment."
            : "Comment persistence and email notifications are planned, but not wired yet.";

        return RedirectToLocal(input.ReturnUrl);
    }

    [HttpPost("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(GalleryInteractionInputModel input)
    {
        var post = await _postRepository.GetByIdAsync(input.PostId);
        if (post == null)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        var currentUserId = TryGetCurrentUserId();
        if (!currentUserId.HasValue || post.UserId != currentUserId.Value)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 403 });
        }

        return RedirectToAction("FeatureNotReady", "Errors", new
        {
            feature = "server-side post deletion",
            missingContract = "DeletePostUseCase",
            returnUrl = input.ReturnUrl
        });
    }

    [HttpGet("Empty")]
    public IActionResult EmptyState()
    {
        return View("Empty", new EmptyGalleryViewModel
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            PrimaryActionText = User.Identity?.IsAuthenticated ?? false ? "Open editor" : "Create an account",
            PrimaryActionUrl = User.Identity?.IsAuthenticated ?? false
                ? Url.Action("Index", "Editor") ?? "/Editor"
                : Url.Action("Register", "Auth") ?? "/Auth/Register",
            SecondaryActionText = "Return to home",
            SecondaryActionUrl = Url.Action("Index", "Home") ?? "/"
        });
    }

    private PaginationViewModel BuildPagination(int currentPage, int totalPages)
    {
        var links = Enumerable.Range(1, totalPages)
            .Select(pageNumber => new PaginationLinkViewModel
            {
                PageNumber = pageNumber,
                IsCurrent = pageNumber == currentPage,
                Url = Url.Action(nameof(Index), new { page = pageNumber }) ?? $"/Gallery?page={pageNumber}"
            })
            .ToList();

        return new PaginationViewModel
        {
            Links = links,
            HasPrevious = currentPage > 1,
            HasNext = currentPage < totalPages,
            PreviousUrl = Url.Action(nameof(Index), new { page = currentPage - 1 }) ?? $"/Gallery?page={currentPage - 1}",
            NextUrl = Url.Action(nameof(Index), new { page = currentPage + 1 }) ?? $"/Gallery?page={currentPage + 1}"
        };
    }

    private GalleryPostCardViewModel MapCard(Post post, int currentPage, int? currentUserId)
    {
        var authorName = string.IsNullOrWhiteSpace(post.User.DisplayName) ? post.User.Username : post.User.DisplayName!;
        return new GalleryPostCardViewModel
        {
            Id = post.Id,
            AuthorName = authorName,
            AuthorHandle = $"@{post.User.Username}",
            Description = string.IsNullOrWhiteSpace(post.Description) ? "No caption provided." : post.Description,
            CoverImageUrl = post.Images.OrderBy(image => image.DisplayOrder).Select(image => image.FilePath).FirstOrDefault() ?? "/images/mock-gallery/orbit-01.svg",
            CreatedLabel = post.CreatedAt.ToString("dd MMM yyyy"),
            LikeCount = post.Likes.Count,
            CommentCount = post.Comments.Count,
            ImageCount = post.Images.Count,
            OpenUrl = Url.Action(nameof(Index), new { page = currentPage, postId = post.Id }) ?? $"/Gallery?page={currentPage}&postId={post.Id}",
            IsOwnedByCurrentUser = currentUserId.HasValue && currentUserId.Value == post.UserId
        };
    }

    private GalleryPostModalViewModel MapModal(Post post, int currentPage, int? currentUserId)
    {
        var authorName = string.IsNullOrWhiteSpace(post.User.DisplayName) ? post.User.Username : post.User.DisplayName!;
        return new GalleryPostModalViewModel
        {
            Id = post.Id,
            AuthorName = authorName,
            AuthorHandle = $"@{post.User.Username}",
            Description = string.IsNullOrWhiteSpace(post.Description) ? "No caption provided." : post.Description,
            CreatedLabel = post.CreatedAt.ToString("dd MMM yyyy 'at' HH:mm"),
            ImageUrls = post.Images.OrderBy(image => image.DisplayOrder).Select(image => image.FilePath).ToList(),
            Comments = post.Comments
                .OrderByDescending(comment => comment.CreatedAt)
                .Select(comment => new GalleryCommentViewModel
                {
                    AuthorName = string.IsNullOrWhiteSpace(comment.User.DisplayName) ? comment.User.Username : comment.User.DisplayName!,
                    AuthorHandle = $"@{comment.User.Username}",
                    Content = comment.Content,
                    CreatedLabel = comment.CreatedAt.ToString("dd MMM yyyy 'at' HH:mm")
                })
                .ToList(),
            LikeCount = post.Likes.Count,
            CommentCount = post.Comments.Count,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            IsOwnedByCurrentUser = currentUserId.HasValue && currentUserId.Value == post.UserId,
            CloseUrl = Url.Action(nameof(Index), new { page = currentPage }) ?? $"/Gallery?page={currentPage}",
            ShareUrl = Url.Action(nameof(Index), "Gallery", new { page = currentPage, postId = post.Id }) ?? $"/Gallery?page={currentPage}&postId={post.Id}",
            LoginUrl = Url.Action("Login", "Auth", new
            {
                returnUrl = Url.Action(nameof(Index), new { page = currentPage, postId = post.Id })
            }) ?? "/Auth/Login",
            LikeActionUrl = Url.Action(nameof(Like), "Gallery") ?? "/Gallery/Like",
            CommentActionUrl = Url.Action(nameof(Comment), "Gallery") ?? "/Gallery/Comment",
            DeleteActionUrl = Url.Action(nameof(Delete), "Gallery") ?? "/Gallery/Delete"
        };
    }

    private int? TryGetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(claim?.Value, out var userId) ? userId : null;
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }
}
