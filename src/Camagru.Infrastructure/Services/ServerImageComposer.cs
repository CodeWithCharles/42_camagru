using Camagru.Application.Contracts.Posts;
using Camagru.Application.Interfaces;
using Camagru.Infrastructure.Options;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Camagru.Infrastructure.Services;

public class ServerImageComposer : IImageComposer
{
    private readonly string _webRootPath;

    public ServerImageComposer(IOptions<StaticAssetOptions> assetOptions)
    {
        _webRootPath = assetOptions.Value.WebRootPath;
    }

    public async Task<byte[]> ComposeAsync(
        Stream baseImageStream,
        IReadOnlyList<OverlayRenderInstruction> overlays,
        CancellationToken cancellationToken = default)
    {
        if (baseImageStream.CanSeek)
        {
            baseImageStream.Position = 0;
        }

        using var baseImage = await Image.LoadAsync<Rgba32>(baseImageStream, cancellationToken);

        foreach (var overlay in overlays.OrderBy(item => item.ZIndex))
        {
            var overlayPhysicalPath = ResolveAssetPath(overlay.AssetPath);
            if (!File.Exists(overlayPhysicalPath))
            {
                throw new FileNotFoundException($"Overlay asset not found: {overlay.AssetPath}", overlayPhysicalPath);
            }

            using var overlayImage = await Image.LoadAsync<Rgba32>(overlayPhysicalPath, cancellationToken);
            overlayImage.Mutate(context =>
            {
                context.Resize(new ResizeOptions
                {
                    Size = new Size(
                        Math.Max(1, (int)Math.Round(overlay.Width)),
                        Math.Max(1, (int)Math.Round(overlay.Height))),
                    Mode = ResizeMode.Stretch
                });

                if (Math.Abs(overlay.RotationDegrees) > 0.01d)
                {
                    context.Rotate((float)overlay.RotationDegrees);
                }
            });

            baseImage.Mutate(context =>
                context.DrawImage(
                    overlayImage,
                    new Point((int)Math.Round(overlay.X), (int)Math.Round(overlay.Y)),
                    1f));
        }

        await using var output = new MemoryStream();
        await baseImage.SaveAsync(output, new PngEncoder(), cancellationToken);
        return output.ToArray();
    }

    private string ResolveAssetPath(string assetPath)
    {
        var trimmedPath = assetPath.TrimStart('/', '\\');
        var normalizedPath = trimmedPath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        return Path.Combine(_webRootPath, normalizedPath);
    }
}
