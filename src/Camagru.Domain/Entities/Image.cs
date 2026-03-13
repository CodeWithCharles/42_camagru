namespace Camagru.Domain.Entities;

public class Image
{
	public int Id { get; set; }
	public int PostId { get; set; }
	public required string FilePath { get; set; }
	public int DisplayOrder { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public Post Post { get; set; } = null!;
}
