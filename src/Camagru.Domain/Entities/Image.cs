namespace Camagru.Domain.Entities;

public class Image
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public required string FilePath { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public User User { get; set; } = null!;
	public ICollection<Comment> Comments { get; set; } = new List<Comment>();
	public ICollection<Like> Likes { get; set; } = new List<Like>();
}
