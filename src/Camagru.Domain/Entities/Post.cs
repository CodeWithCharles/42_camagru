namespace Camagru.Domain.Entities;

public class Post
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public string Description { get; set; } = null!;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public User User { get; set; } = null!;
	public ICollection<Image> Images { get; set; } = new List<Image>();
	public ICollection<Comment> Comments { get; set; } = new List<Comment>();
	public ICollection<Like> Likes { get; set; } = new List<Like>();
}