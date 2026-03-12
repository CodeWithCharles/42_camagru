using Camagru.Infrastructure.DependencyInjection;
using Camagru.Infrastructure.Persistence.Init;
using DotNetEnv;

// Only load .env when running locally (not in Docker)
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddPersistenceServices();    
} catch (Exception)
{
    throw;
}

builder.Services.AddControllersWithViews();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();