using Camagru.Web.Models.Editor;

namespace Camagru.Web.Services;

public class StickerCatalogService
{
    private readonly IWebHostEnvironment _environment;

    public StickerCatalogService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IReadOnlyList<StickerGroupViewModel> GetStickerGroups()
    {
        var stickerPath = Path.Combine(_environment.WebRootPath, "images", "stickers");
        if (!Directory.Exists(stickerPath))
        {
            return [];
        }

        var allStickers = Directory.EnumerateFiles(stickerPath, "*.png", SearchOption.TopDirectoryOnly)
            .Select((filePath, index) =>
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                return new StickerViewModel
                {
                    Id = $"sticker-{index + 1}",
                    OverlayId = index + 1,
                    Name = ToTitleCase(fileName),
                    Category = fileName is "hearts" or "stars" ? "Pulse" : "Signals",
                    ImageUrl = $"/images/stickers/{Path.GetFileName(filePath)}",
                    Accent = fileName switch
                    {
                        "hearts" => "var(--color-accent-coral)",
                        "stars" => "var(--color-accent-cyan)",
                        _ => "var(--color-accent-lime)"
                    }
                };
            })
            .ToList();

        return allStickers
            .GroupBy(sticker => sticker.Category)
            .Select(group => new StickerGroupViewModel
            {
                Id = group.Key.ToLowerInvariant(),
                Title = group.Key,
                Description = group.Key == "Pulse"
                    ? "Reactive neon markers for playful montages."
                    : "Signal overlays for comic and alert moments.",
                Stickers = group.ToList()
            })
            .ToList();
    }

    private static string ToTitleCase(string value)
    {
        return string.Join(" ", value.Split('-', '_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}
