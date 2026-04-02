using Camagru.Application.DependencyInjection;
using Camagru.Infrastructure.DependencyInjection;
using Camagru.Infrastructure.Options;
using Camagru.Infrastructure.Persistence.Init;
using Camagru.Web.Options;
using Camagru.Web.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistenceServices();
builder.Services.AddSmtpServices();
builder.Services.Configure<EditorUploadOptions>(builder.Configuration.GetSection(EditorUploadOptions.SectionName));
builder.Services.AddScoped<StickerCatalogService>();
builder.Services.AddSingleton(new UiFeatureFlags
{
    EnableConfirmationResend = true,
    EnableGalleryPersistence = true,
    EnableEditorPublish = true
});
builder.Services.Configure<StaticAssetOptions>(options =>
{
    options.WebRootPath = builder.Environment.WebRootPath;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Errors/403";
        options.ReturnUrlParameter = "returnUrl";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.Configure<CookieTempDataProviderOptions>(options =>
{
    options.Cookie.Name = ".Camagru.TempData";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".Camagru.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var dataProtectionDirectory = ResolveDataProtectionDirectory(builder.Environment);
Directory.CreateDirectory(dataProtectionDirectory);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionDirectory))
    .SetApplicationName("Camagru");

builder.Services.AddControllersWithViews();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

var uploadsOptions = app.Services.GetRequiredService<IOptions<UploadsOptions>>();
var uploadsPath = uploadsOptions.Value.DirectoryPath;
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Errors/500");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/Errors/{0}");
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Gallery}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

static string ResolveDataProtectionDirectory(IHostEnvironment environment)
{
    var configuredPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_DIRECTORY");
    if (!string.IsNullOrWhiteSpace(configuredPath))
    {
        return configuredPath;
    }

    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        return "/root/.aspnet/DataProtection-Keys";
    }

    return Path.Combine(environment.ContentRootPath, ".aspnet", "DataProtection-Keys");
}
