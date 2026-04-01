using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class PostsController : Controller
{
    [HttpGet("Create")]
    [Authorize]
    public IActionResult Create()
    {
        return RedirectToAction("Index", "Editor");
    }

    [HttpGet("GetOverlays")]
    public IActionResult GetOverlays()
    {
        return RedirectToAction("FeatureNotReady", "Errors", new
        {
            feature = "overlay feed endpoint",
            missingContract = "EditorOverlayCatalogApi"
        });
    }

    [HttpPost("Create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult CreateLegacy()
    {
        return RedirectToAction("FeatureNotReady", "Errors", new
        {
            feature = "server-side publishing from the editor",
            missingContract = "PublishEditorCompositionUseCase",
            returnUrl = Url.Action("Index", "Editor")
        });
    }
}
