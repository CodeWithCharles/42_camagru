using System.Security.Claims;
using Camagru.Web.Models.Editor;
using Camagru.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class EditorController : Controller
{
    private readonly StickerCatalogService _stickerCatalogService;

    public EditorController(StickerCatalogService stickerCatalogService)
    {
        _stickerCatalogService = stickerCatalogService;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            TempData["Toast.Info"] = "Log in to access the montage editor and your private capture tray.";
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(nameof(Index), "Editor") });
        }

        return View(new EditorPageViewModel
        {
            StorageKey = $"camagru-editor-drafts-{User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "guest"}",
            StickerGroups = _stickerCatalogService.GetStickerGroups(),
            Stage = new EditorStageViewModel
            {
                EmptyTitle = "No base image selected",
                EmptyDescription = "Start the webcam or upload a photo to stage your next montage.",
                PreviewNote = "Preview exports are generated in-browser for UI validation only. Final publishing remains a future server-side use case."
            }
        });
    }
}
