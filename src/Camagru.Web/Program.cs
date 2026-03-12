using Camagru.Domain.Interfaces;
using Camagru.Infrastructure.Persistence;
using Camagru.Infrastructure.Persistence.Init;
using Camagru.Web.Options;
using Microsoft.EntityFrameworkCore;

// Only load .env when running locally (not in Docker)
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    DotNetEnv.Env.Load();
}

var builder = WebApplication.CreateBuilder(args);

// Bind and validate Postgres options
var postgresOptions = new PostgresOptions
{
    Host = Environment.GetEnvironmentVariable("POSTGRES_HOST")
        ?? throw new InvalidOperationException("Missing env var: POSTGRES_HOST"),
    Port = Environment.GetEnvironmentVariable("POSTGRES_PORT")
        ?? throw new InvalidOperationException("Missing env var: POSTGRES_PORT"),
    Database = Environment.GetEnvironmentVariable("POSTGRES_DB")
        ?? throw new InvalidOperationException("Missing env var: POSTGRES_DB"),
    Username = Environment.GetEnvironmentVariable("POSTGRES_USER")
        ?? throw new InvalidOperationException("Missing env var: POSTGRES_USER"),
    Password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
        ?? throw new InvalidOperationException("Missing env var: POSTGRES_PASSWORD"),
};

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(postgresOptions.ToConnectionString()));

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