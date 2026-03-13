using System.Security.Claims;
using Camagru.Application.Contracts.Posts;
using Camagru.Application.UseCases.Posts;
using Camagru.Infrastructure.UseCases.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class PostsController : Controller
{
    private readonly GetAvailableOverlaysUseCase _getAvailableOverlaysUseCase;
    private readonly CreatePostUseCase _createPostUseCase;

    public PostsController(
        GetAvailableOverlaysUseCase getAvailableOverlaysUseCase,
        CreatePostUseCase createPostUseCase)
    {
        _getAvailableOverlaysUseCase = getAvailableOverlaysUseCase;
        _createPostUseCase = createPostUseCase;
    }

    // GET /Posts/Create - Show post creation form
    [HttpGet("Create")]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var result = await _getAvailableOverlaysUseCase.ExecuteAsync();
        if (!result.Success)
            return BadRequest(result.Error);

        ViewBag.Overlays = result.Data;
        return View();
    }

    // GET /Posts/GetOverlays - Fetch overlays as JSON (for AJAX)
    [HttpGet("GetOverlays")]
    public async Task<IActionResult> GetOverlays()
    {
        var result = await _getAvailableOverlaysUseCase.ExecuteAsync();
        if (!result.Success)
            return Json(new { success = false, error = result.Error });

        return Json(new { success = true, overlays = result.Data });
    }

    // POST /Posts/Create - Create post with images and overlays (AJAX)
    [HttpPost("Create")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Json(new { success = false, error = "Unauthorized" });

        if (!ModelState.IsValid)
            return Json(new { success = false, error = "Invalid request data" });

        var result = await _createPostUseCase.ExecuteAsync(userId, request);
        if (!result.Success)
            return Json(new { success = false, error = result.Error });

        return Json(new { success = true, postId = result.Data!.PostId, message = result.Data!.Message });
    }
}
