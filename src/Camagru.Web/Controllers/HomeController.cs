using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Camagru.Web.Models;
using Camagru.Application.Interfaces;

namespace Camagru.Web.Controllers;

public class HomeController : Controller
{
    private readonly IEmailSender _emailSender;

    public HomeController(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("test-email")]
    public async Task<IActionResult> TestEmail()
    {
        try
        {
            await _emailSender.SendAsync(
                "test@test.com",
                "Test Email from Camagru",
                "<h1>Hello from Camagru!</h1><p>This is a test email with <strong>HTML</strong> support.</p>"
            );
            return Content("Email sent successfully! Check Mailpit at http://localhost:8025", "text/plain");
        }
        catch (Exception ex)
        {
            return Content($"Failed to send email: {ex.Message}", "text/plain");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

