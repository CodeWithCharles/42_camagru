using System.Security.Claims;
using System.Text.Json;
using Camagru.Application.Contracts.Posts;
using Camagru.Application.UseCases.Posts;
using Camagru.Web.Models.Editor;
using Camagru.Web.Options;
using Camagru.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Camagru.Web.Controllers;

[Authorize]
[Route("[controller]")]
public class EditorController : Controller
{
    private readonly GetAvailableOverlaysUseCase _getAvailableOverlaysUseCase;
    private readonly PublishMontageUseCase _publishMontageUseCase;
    private readonly StickerCatalogService _stickerCatalogService;
    private readonly EditorUploadOptions _editorUploadOptions;

    public EditorController(
        GetAvailableOverlaysUseCase getAvailableOverlaysUseCase,
        PublishMontageUseCase publishMontageUseCase,
        StickerCatalogService stickerCatalogService,
        IOptions<EditorUploadOptions> editorUploadOptions)
    {
        _getAvailableOverlaysUseCase = getAvailableOverlaysUseCase;
        _publishMontageUseCase = publishMontageUseCase;
        _stickerCatalogService = stickerCatalogService;
        _editorUploadOptions = editorUploadOptions.Value;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var overlayResult = await _getAvailableOverlaysUseCase.ExecuteAsync();
        var stickerGroups = overlayResult.Success && overlayResult.Data != null && overlayResult.Data.Count > 0
            ? overlayResult.Data
                .GroupBy(sticker => sticker.Category)
                .Select(group => new StickerGroupViewModel
                {
                    Id = group.Key.ToLowerInvariant(),
                    Title = group.Key,
                    Description = group.Key == "Hearts"
                        ? "Reactive neon markers for playful montages."
                        : "Server-backed overlays ready for final composition.",
                    Stickers = group.Select(sticker => new StickerViewModel
                    {
                        Id = sticker.Id.ToString(),
                        OverlayId = sticker.Id,
                        Name = sticker.Name,
                        Category = sticker.Category,
                        ImageUrl = sticker.FilePath,
                        Accent = sticker.Category switch
                        {
                            "Hearts" => "var(--color-accent-coral)",
                            "Stars" => "var(--color-accent-cyan)",
                            _ => "var(--color-accent-lime)"
                        }
                    }).ToList()
                })
                .ToList()
            : _stickerCatalogService.GetStickerGroups();

        return View(new EditorPageViewModel
        {
            StorageKey = $"camagru-editor-drafts-{User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest"}",
            StickerGroups = stickerGroups,
            Stage = new EditorStageViewModel
            {
                EmptyTitle = "No base image selected",
                EmptyDescription = "Start the webcam or upload a photo to stage your next montage.",
                PreviewNote = "Preview exports remain local for instant feedback, while Publish now submits the base image and overlay payload for server-side composition.",
                PublishActionUrl = Url.Action(nameof(Publish), "Editor") ?? "/Editor/Publish"
            }
        });
    }

    [HttpPost("Publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(EditorPublishInputModel input, CancellationToken cancellationToken)
    {
        var uploadValidationError = ValidateBaseImage(input.BaseImage);
        if (uploadValidationError != null)
        {
            TempData["Toast.Error"] = uploadValidationError;
            return RedirectToAction(nameof(Index));
        }

        var payloadValidationError = ValidateOverlayPayload(input.OverlayPayloadJson);
        if (payloadValidationError != null)
        {
            TempData["Toast.Error"] = payloadValidationError;
            return RedirectToAction(nameof(Index));
        }

        EditorCompositionPayloadInputModel? payload;
        try
        {
            payload = JsonSerializer.Deserialize<EditorCompositionPayloadInputModel>(
                input.OverlayPayloadJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException)
        {
            TempData["Toast.Error"] = "The composition payload could not be parsed.";
            return RedirectToAction(nameof(Index));
        }

        if (payload == null || payload.Overlays.Count == 0)
        {
            TempData["Toast.Error"] = "At least one overlay is required for publishing.";
            return RedirectToAction(nameof(Index));
        }

        if (payload.Overlays.Any(overlay =>
                overlay.OverlayId <= 0 ||
                !IsFiniteCoordinate(overlay.X) ||
                !IsFiniteCoordinate(overlay.Y) ||
                !IsFiniteSize(overlay.Width) ||
                !IsFiniteSize(overlay.Height)))
        {
            TempData["Toast.Error"] = "One or more overlays contain invalid dimensions or coordinates.";
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            TempData["Toast.Error"] = "Your session is missing identity information.";
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Editor") });
        }

        var baseImage = input.BaseImage!;
        await using var stream = baseImage.OpenReadStream();
        var result = await _publishMontageUseCase.ExecuteAsync(new PublishMontageRequest
        {
            UserId = parsedUserId,
            Description = input.Description ?? string.Empty,
            BaseImageStream = stream,
            BaseImageFileName = baseImage.FileName,
            Overlays = payload.Overlays.Select(overlay => new PublishMontageOverlayInstruction
            {
                OverlayId = overlay.OverlayId,
                X = overlay.X,
                Y = overlay.Y,
                Width = overlay.Width,
                Height = overlay.Height,
                ZIndex = overlay.ZIndex,
                RotationDegrees = overlay.RotationDegrees
            }).ToList()
        }, cancellationToken);

        if (!result.Success || result.Data == null)
        {
            TempData["Toast.Error"] = result.Error ?? "Publishing failed.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Toast.Success"] = "Montage published to the public gallery.";
        return RedirectToAction("Index", "Gallery", new { page = 1, postId = result.Data.PostId });
    }

    private string? ValidateBaseImage(IFormFile? baseImage)
    {
        if (baseImage == null || baseImage.Length == 0)
        {
            return "Capture or upload a base image before publishing.";
        }

        if (baseImage.Length > _editorUploadOptions.MaxBytes)
        {
            return $"Base images must be {_editorUploadOptions.MaxBytes / (1024 * 1024)} MB or smaller.";
        }

        var extension = Path.GetExtension(baseImage.FileName);
        if (string.IsNullOrWhiteSpace(extension) ||
            !_editorUploadOptions.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return "Only PNG and JPEG images can be published.";
        }

        var normalizedContentType = baseImage.ContentType?.Split(';', StringSplitOptions.RemoveEmptyEntries)[0].Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedContentType) ||
            !_editorUploadOptions.AllowedContentTypes.Contains(normalizedContentType, StringComparer.OrdinalIgnoreCase))
        {
            return "The uploaded file must be a PNG or JPEG image.";
        }

        return null;
    }

    private string? ValidateOverlayPayload(string? overlayPayloadJson)
    {
        if (string.IsNullOrWhiteSpace(overlayPayloadJson))
        {
            return "Add at least one overlay before publishing.";
        }

        if (overlayPayloadJson.Length > _editorUploadOptions.MaxOverlayPayloadCharacters)
        {
            return "The composition payload is too large. Reduce the number of overlays and try again.";
        }

        return null;
    }

    private static bool IsFiniteCoordinate(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }

    private static bool IsFiniteSize(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }
}
