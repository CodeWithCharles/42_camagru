using Microsoft.AspNetCore.Mvc;

namespace Camagru.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Gallery");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return RedirectToAction("Status", "Errors", new { statusCode = 500 });
    }
}
