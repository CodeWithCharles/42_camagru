namespace Camagru.Domain.Entities;

public class Comment
{
	public int Id { get; set; }
	public int PostId { get; set; }
	public int UserId { get; set; }
	public required string Content { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public Post Post { get; set; } = null!;
	public User User { get; set; } = null!;
}
