using System.Security.Cryptography;
using Camagru.Application.Interfaces;
using Camagru.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Camagru.Infrastructure.Services;

public class LocalImageStorage : IImageStorage
{
    private readonly string _uploadsPath;

    public LocalImageStorage(IOptions<UploadsOptions> uploadsOptions)
    {
        _uploadsPath = uploadsOptions.Value.DirectoryPath;
        Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<string> SaveImageAsync(
        ReadOnlyMemory<byte> imageBytes,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var normalizedExtension = string.IsNullOrWhiteSpace(extension)
            ? ".png"
            : extension.StartsWith(".", StringComparison.Ordinal) ? extension : $".{extension}";

        var fileName = $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant()}{normalizedExtension}";
        var fullPath = Path.Combine(_uploadsPath, fileName);
        await File.WriteAllBytesAsync(fullPath, imageBytes.ToArray(), cancellationToken);
        return $"/uploads/{fileName}";
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return Task.CompletedTask;
        }

        var fileName = Path.GetFileName(relativePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Task.CompletedTask;
        }

        var fullPath = Path.Combine(_uploadsPath, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
