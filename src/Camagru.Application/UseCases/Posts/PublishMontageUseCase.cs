using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Application.Interfaces;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Application.UseCases.Posts;

public class PublishMontageUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IOverlayRepository _overlayRepository;
    private readonly IPostRepository _postRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IImageComposer _imageComposer;
    private readonly IImageStorage _imageStorage;

    public PublishMontageUseCase(
        IUserRepository userRepository,
        IOverlayRepository overlayRepository,
        IPostRepository postRepository,
        IImageRepository imageRepository,
        IImageComposer imageComposer,
        IImageStorage imageStorage)
    {
        _userRepository = userRepository;
        _overlayRepository = overlayRepository;
        _postRepository = postRepository;
        _imageRepository = imageRepository;
        _imageComposer = imageComposer;
        _imageStorage = imageStorage;
    }

    public async Task<ServiceResult<PublishMontageResponse>> ExecuteAsync(PublishMontageRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _userRepository.ExistsAsync(request.UserId))
        {
            return ServiceResult<PublishMontageResponse>.Fail("User not found");
        }

        if (request.BaseImageStream == Stream.Null || !request.BaseImageStream.CanRead)
        {
            return ServiceResult<PublishMontageResponse>.Fail("A base image is required");
        }

        if (request.Overlays.Count == 0)
        {
            return ServiceResult<PublishMontageResponse>.Fail("At least one overlay is required");
        }

        if (request.Description.Length > 2_000)
        {
            return ServiceResult<PublishMontageResponse>.Fail("Description must not exceed 2000 characters");
        }

        var renderInstructions = new List<OverlayRenderInstruction>(request.Overlays.Count);
        foreach (var overlay in request.Overlays.OrderBy(item => item.ZIndex))
        {
            var overlayEntity = await _overlayRepository.GetByIdAsync(overlay.OverlayId);
            if (overlayEntity == null)
            {
                return ServiceResult<PublishMontageResponse>.Fail($"Overlay {overlay.OverlayId} was not found");
            }

            if (overlay.Width <= 0 || overlay.Height <= 0)
            {
                return ServiceResult<PublishMontageResponse>.Fail("Overlay dimensions must be greater than zero");
            }

            renderInstructions.Add(new OverlayRenderInstruction
            {
                AssetPath = overlayEntity.FilePath,
                X = overlay.X,
                Y = overlay.Y,
                Width = overlay.Width,
                Height = overlay.Height,
                ZIndex = overlay.ZIndex,
                RotationDegrees = overlay.RotationDegrees
            });
        }

        byte[] composedBytes;
        try
        {
            composedBytes = await _imageComposer.ComposeAsync(request.BaseImageStream, renderInstructions, cancellationToken);
        }
        catch (Exception ex)
        {
            return ServiceResult<PublishMontageResponse>.Fail($"Failed to compose montage: {ex.Message}");
        }

        var imageUrl = await _imageStorage.SaveImageAsync(composedBytes, ".png", cancellationToken);

        var post = new Post
        {
            UserId = request.UserId,
            Description = request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _postRepository.AddAsync(post);

        await _imageRepository.AddAsync(new Image
        {
            PostId = post.Id,
            FilePath = imageUrl,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow
        });

        return ServiceResult<PublishMontageResponse>.Ok(new PublishMontageResponse
        {
            PostId = post.Id,
            ImageUrls = [imageUrl]
        });
    }
}
