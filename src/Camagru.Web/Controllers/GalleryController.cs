using System.Security.Claims;
using Camagru.Application.Contracts.Posts;
using Camagru.Application.UseCases.Posts;
using Camagru.Web.Models.Gallery;
using Camagru.Web.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class GalleryController : Controller
{
    private const int PageSize = 6;
    private readonly ListGalleryPostsUseCase _listGalleryPostsUseCase;
    private readonly GetPostDetailsUseCase _getPostDetailsUseCase;
    private readonly ToggleLikeUseCase _toggleLikeUseCase;
    private readonly AddCommentUseCase _addCommentUseCase;
    private readonly DeletePostUseCase _deletePostUseCase;

    public GalleryController(
        ListGalleryPostsUseCase listGalleryPostsUseCase,
        GetPostDetailsUseCase getPostDetailsUseCase,
        ToggleLikeUseCase toggleLikeUseCase,
        AddCommentUseCase addCommentUseCase,
        DeletePostUseCase deletePostUseCase)
    {
        _listGalleryPostsUseCase = listGalleryPostsUseCase;
        _getPostDetailsUseCase = getPostDetailsUseCase;
        _toggleLikeUseCase = toggleLikeUseCase;
        _addCommentUseCase = addCommentUseCase;
        _deletePostUseCase = deletePostUseCase;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1, int? postId = null)
    {
        if (page < 1)
        {
            return RedirectToAction(nameof(Index), new { page = 1 });
        }

        var currentUserId = TryGetCurrentUserId();
        var listResult = await _listGalleryPostsUseCase.ExecuteAsync(new ListGalleryPostsRequest
        {
            Page = page,
            PageSize = PageSize,
            ViewerUserId = currentUserId
        });

        if (!listResult.Success || listResult.Data == null)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 500 });
        }

        if (listResult.Data.TotalCount == 0)
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

        var totalPages = (int)Math.Ceiling(listResult.Data.TotalCount / (double)PageSize);
        if (page > totalPages)
        {
            return RedirectToAction("Status", "Errors", new { statusCode = 404 });
        }

        GalleryPostDetailsDto? selectedPost = null;
        if (postId.HasValue)
        {
            var postDetailsResult = await _getPostDetailsUseCase.ExecuteAsync(new GetPostDetailsRequest
            {
                PostId = postId.Value,
                ViewerUserId = currentUserId
            });

            if (!postDetailsResult.Success || postDetailsResult.Data == null)
            {
                return RedirectToAction("Status", "Errors", new { statusCode = 404 });
            }

            selectedPost = postDetailsResult.Data;
        }

        var model = new GalleryIndexViewModel
        {
            CurrentPage = page,
            TotalItems = listResult.Data.TotalCount,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            LoginUrl = Url.Action("Login", "Auth", new
            {
                returnUrl = Url.Action(nameof(Index), new { page, postId })
            }) ?? "/Auth/Login",
            Posts = listResult.Data.Posts.Select(post => MapCard(post, page)).ToList(),
            ActivePost = selectedPost == null ? null : MapModal(selectedPost, page),
            Pagination = BuildPagination(page, totalPages)
        };

        return View(model);
    }

    [HttpPost("Like")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(GalleryInteractionInputModel input)
    {
        var userId = TryGetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = input.ReturnUrl });
        }

        var result = await _toggleLikeUseCase.ExecuteAsync(new ToggleLikeRequest
        {
            PostId = input.PostId,
            UserId = userId.Value
        });

        if (!result.Success)
        {
            return RedirectToAction("Status", "Errors", new
            {
                statusCode = string.Equals(result.Error, "Post not found", StringComparison.OrdinalIgnoreCase) ? 404 : 403
            });
        }

        TempData["Toast.Success"] = result.Data?.IsLiked == true ? "Post liked." : "Like removed.";
        return RedirectToLocal(input.ReturnUrl);
    }

    [HttpPost("Comment")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(GalleryInteractionInputModel input)
    {
        var userId = TryGetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = input.ReturnUrl });
        }

        var result = await _addCommentUseCase.ExecuteAsync(new AddCommentRequest
        {
            PostId = input.PostId,
            UserId = userId.Value,
            Text = input.Comment ?? string.Empty
        });

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Post not found", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Status", "Errors", new { statusCode = 404 });
            }

            TempData["Toast.Error"] = result.Error ?? "Comment submission failed.";
            return RedirectToLocal(input.ReturnUrl);
        }

        TempData[result.Data?.WarningMessage == null ? "Toast.Success" : "Toast.Info"] =
            result.Data?.WarningMessage ?? "Comment posted.";

        return RedirectToLocal(input.ReturnUrl);
    }

    [HttpPost("Delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(GalleryInteractionInputModel input)
    {
        var userId = TryGetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = input.ReturnUrl });
        }

        var result = await _deletePostUseCase.ExecuteAsync(new DeletePostRequest
        {
            PostId = input.PostId,
            RequestingUserId = userId.Value
        });

        if (!result.Success)
        {
            return RedirectToAction("Status", "Errors", new
            {
                statusCode = string.Equals(result.Error, "Post not found", StringComparison.OrdinalIgnoreCase) ? 404 : 403
            });
        }

        TempData["Toast.Success"] = "Post deleted.";
        return RedirectToAction(nameof(Index), new { page = input.Page < 1 ? 1 : input.Page });
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

    private GalleryPostCardViewModel MapCard(GalleryPostSummaryDto post, int currentPage)
    {
        return new GalleryPostCardViewModel
        {
            Id = post.Id,
            AuthorName = post.AuthorName,
            AuthorHandle = $"@{post.AuthorUsername}",
            Description = post.Description,
            CoverImageUrl = post.ImageUrls.FirstOrDefault() ?? "/images/mock-gallery/orbit-01.svg",
            CreatedLabel = post.CreatedAt.ToString("dd MMM yyyy"),
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            ImageCount = post.ImageUrls.Count,
            OpenUrl = Url.Action(nameof(Index), new { page = currentPage, postId = post.Id }) ?? $"/Gallery?page={currentPage}&postId={post.Id}",
            IsOwnedByCurrentUser = post.IsOwnedByViewer,
            IsLikedByCurrentUser = post.IsLikedByViewer
        };
    }

    private GalleryPostModalViewModel MapModal(GalleryPostDetailsDto post, int currentPage)
    {
        return new GalleryPostModalViewModel
        {
            Id = post.Id,
            AuthorName = post.AuthorName,
            AuthorHandle = $"@{post.AuthorUsername}",
            Description = post.Description,
            CreatedLabel = post.CreatedAt.ToString("dd MMM yyyy 'at' HH:mm"),
            ImageUrls = post.ImageUrls,
            Comments = post.Comments
                .Select(comment => new GalleryCommentViewModel
                {
                    AuthorName = comment.AuthorName,
                    AuthorHandle = $"@{comment.AuthorUsername}",
                    Content = comment.Content,
                    CreatedLabel = comment.CreatedAt.ToString("dd MMM yyyy 'at' HH:mm")
                })
                .ToList(),
            LikeCount = post.LikeCount,
            CommentCount = post.CommentCount,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            IsOwnedByCurrentUser = post.IsOwnedByViewer,
            IsLikedByCurrentUser = post.IsLikedByViewer,
            CurrentPage = currentPage,
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
