namespace Camagru.Application.Contracts.Posts;

public class OverlayDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string FilePath { get; set; }
    public int DisplayOrder { get; set; }
}
