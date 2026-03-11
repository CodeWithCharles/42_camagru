namespace Camagru.Domain.Entities;

public class Like
{
	public int Id { get; set; }
	public int ImageId { get; set; }
	public int UserId { get; set; }
	public Image Image { get; set; } = null!;
	public User User { get; set; } = null!;
}
