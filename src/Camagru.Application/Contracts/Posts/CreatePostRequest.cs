namespace Camagru.Application.Contracts.Posts;

public class CreatePostRequest
{
    public required string Description { get; set; } = string.Empty;
    public required List<string> Images { get; set; } = new(); // List of base64-encoded JPEG images (pre-composited)
}
