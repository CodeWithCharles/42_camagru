namespace Camagru.Application.Contracts.Posts;

public class PostCreationResponse
{
    public int PostId { get; set; }
    public List<int> ImageIds { get; set; } = new();
    public required string Message { get; set; }
}
