using Camagru.Application.Contracts.Common;
using Camagru.Application.Contracts.Posts;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Camagru.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Camagru.Infrastructure.UseCases.Posts;

public class CreatePostUseCase
{
    private readonly IPostRepository _postRepository;
    private readonly IImageRepository _imageRepository;
    private readonly IUserRepository _userRepository;
    private readonly string _uploadsPath;

    public CreatePostUseCase(
        IPostRepository postRepository,
        IImageRepository imageRepository,
        IUserRepository userRepository,
        IOptions<UploadsOptions> uploadsOptions)
    {
        _postRepository = postRepository;
        _imageRepository = imageRepository;
        _userRepository = userRepository;

        // Use configured uploads path
        _uploadsPath = uploadsOptions.Value.DirectoryPath;
        Directory.CreateDirectory(_uploadsPath);
    }

    public async Task<ServiceResult<PostCreationResponse>> ExecuteAsync(int userId, CreatePostRequest request)
    {
        // Validate user exists
        if (!await _userRepository.ExistsAsync(userId))
            return ServiceResult<PostCreationResponse>.Fail("User not found");

        // Validate at least one image
        if (request.Images == null || request.Images.Count == 0)
            return ServiceResult<PostCreationResponse>.Fail("At least one image is required");

        // Validate max 10 images
        if (request.Images.Count > 10)
            return ServiceResult<PostCreationResponse>.Fail("Maximum 10 images per post");

        try
        {
            // Create post
            var post = new Post
            {
                UserId = userId,
                Description = request.Description ?? string.Empty
            };
            await _postRepository.AddAsync(post);

            var imageIds = new List<int>();

            // Process each pre-composited image (already baked client-side with overlays)
            for (int i = 0; i < request.Images.Count; i++)
            {
                var imageBase64 = request.Images[i];

                try
                {
                    // Save pre-composited image to disk
                    var filePath = await SaveImageFromBase64(imageBase64);

                    // Create image record
                    var image = new Domain.Entities.Image
                    {
                        PostId = post.Id,
                        FilePath = filePath,
                        DisplayOrder = i
                    };
                    await _imageRepository.AddAsync(image);
                    imageIds.Add(image.Id);
                }
                catch (Exception ex)
                {
                    return ServiceResult<PostCreationResponse>.Fail($"Error processing image {i + 1}: {ex.Message}");
                }
            }

            return ServiceResult<PostCreationResponse>.Ok(new PostCreationResponse
            {
                PostId = post.Id,
                ImageIds = imageIds,
                Message = "Post created successfully"
            });
        }
        catch (Exception ex)
        {
            return ServiceResult<PostCreationResponse>.Fail($"Error creating post: {ex.Message}");
        }
    }

    private async Task<string> SaveImageFromBase64(string imageBase64)
    {
        try
        {
            // Remove data URI scheme if present
            var base64Data = imageBase64.Contains(",") 
                ? imageBase64.Split(",")[1] 
                : imageBase64;

            // Decode base64 to bytes
            var imageBytes = Convert.FromBase64String(base64Data);

            // Save to disk
            var fileName = $"{Guid.NewGuid():N}.jpg";
            var filePath = Path.Combine(_uploadsPath, fileName);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return $"/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error saving image: {ex.Message}", ex);
        }
    }
}
