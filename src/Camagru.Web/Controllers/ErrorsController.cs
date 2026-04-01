using Camagru.Web.Models.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

[Route("[controller]")]
public class ErrorsController : Controller
{
    [HttpGet("{statusCode:int}")]
    public IActionResult Status(int statusCode)
    {
        Response.StatusCode = statusCode;
        return View(new StatusPageViewModel
        {
            StatusCode = statusCode,
            Title = statusCode switch
            {
                403 => "Access Forbidden",
                404 => "Route Not Found",
                _ => "Unexpected Error"
            },
            Headline = statusCode switch
            {
                403 => "This corridor is locked",
                404 => "We lost the route beacon",
                _ => "Camagru hit turbulence"
            },
            Description = statusCode switch
            {
                403 => "The request reached Camagru, but this area is reserved for a different user or permission level.",
                404 => "The page or resource you requested does not exist in this build of the frontend.",
                _ => "Something unexpected happened while assembling the page. Try again, or return to the gallery."
            },
            PrimaryActionText = statusCode == 403 ? "Go to gallery" : "Open gallery",
            PrimaryActionUrl = Url.Action("Index", "Gallery") ?? "/Gallery",
            SecondaryActionText = statusCode == 403 ? "Sign in" : "Open profile",
            SecondaryActionUrl = statusCode == 403
                ? Url.Action("Login", "Auth") ?? "/Auth/Login"
                : Url.Action("Index", "Profile") ?? "/Profile"
        });
    }

    [HttpGet("FeatureNotReady")]
    public IActionResult FeatureNotReady(string? feature = null, string? missingContract = null, string? returnUrl = null)
    {
        return View(new StatusPageViewModel
        {
            Title = "Feature Not Wired Yet",
            Headline = "UI pathway staged, backend contract pending",
            Description = "This screen is intentionally interactive at the Web layer, but the persistence or orchestration contract is not available yet.",
            FeatureName = feature,
            MissingContractName = missingContract,
            PrimaryActionText = "Return to previous page",
            PrimaryActionUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Gallery") ?? "/Gallery",
            SecondaryActionText = "Open README notes",
            SecondaryActionUrl = Url.Action("Index", "Gallery") ?? "/Gallery"
        });
    }
}
