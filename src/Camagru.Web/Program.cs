using Camagru.Application.DependencyInjection;
using Camagru.Infrastructure.DependencyInjection;
using Camagru.Infrastructure.Options;
using Camagru.Infrastructure.Persistence.Init;
using Camagru.Web.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddPersistenceServices();
builder.Services.AddSmtpServices();
builder.Services.AddScoped<StickerCatalogService>();
builder.Services.AddSingleton(new UiFeatureFlags
{
    EnableConfirmationResend = false,
    EnableGalleryPersistence = false,
    EnableEditorPublish = false
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Errors/403";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

var dataProtectionDirectory = Path.Combine(builder.Environment.ContentRootPath, ".aspnet", "DataProtection-Keys");
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
