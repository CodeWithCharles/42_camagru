using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Application.Interfaces;
using Camagru.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Camagru.Application.UseCases.Posts;

public class DeletePostUseCase
{
    private readonly IPostRepository _postRepository;
    private readonly IImageStorage _imageStorage;
    private readonly ILogger<DeletePostUseCase> _logger;

    public DeletePostUseCase(
        IPostRepository postRepository,
        IImageStorage imageStorage,
        ILogger<DeletePostUseCase> logger)
    {
        _postRepository = postRepository;
        _imageStorage = imageStorage;
        _logger = logger;
    }

    public async Task<ServiceResult> ExecuteAsync(DeletePostRequest request)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(request.PostId);
        if (post == null)
        {
            return ServiceResult.Fail("Post not found");
        }

        if (post.UserId != request.RequestingUserId)
        {
            return ServiceResult.Fail("Forbidden");
        }

        foreach (var image in post.Images)
        {
            try
            {
                await _imageStorage.DeleteAsync(image.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete image asset {ImagePath} for post {PostId}", image.FilePath, post.Id);
            }
        }

        await _postRepository.DeleteAsync(post);
        return ServiceResult.Ok();
    }
}
