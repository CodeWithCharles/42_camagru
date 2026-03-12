namespace Camagru.Domain.Entities;

public class User
{
	public int Id { get; set; }
	public required string Username { get; set; }
	public required string Email { get; set; }
	public required string PasswordHash { get; set; }
	public bool IsConfirmed { get; set; } = false;
	public string? ConfirmationToken { get; set; }
	public string? ResetToken { get; set; }
	public DateTime? ResetTokenExpiry { get; set; }
	public bool EmailNotificationsEnabled { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public ICollection<Post> Posts { get; set; } = new List<Post>();
	public ICollection<Image> Images { get; set; } = new List<Image>();
	public ICollection<Comment> Comments { get; set; } = new List<Comment>();
	public ICollection<Like> Likes { get; set; } = new List<Like>();
}
