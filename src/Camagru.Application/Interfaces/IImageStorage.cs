namespace Camagru.Application.Interfaces;

public interface IImageStorage
{
    Task<string> SaveImageAsync(
        ReadOnlyMemory<byte> imageBytes,
        string extension,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
