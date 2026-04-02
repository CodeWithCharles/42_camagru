using Camagru.Application.Contracts.Posts;

namespace Camagru.Application.Interfaces;

public interface IImageComposer
{
    Task<byte[]> ComposeAsync(
        Stream baseImageStream,
        IReadOnlyList<OverlayRenderInstruction> overlays,
        CancellationToken cancellationToken = default);
}
