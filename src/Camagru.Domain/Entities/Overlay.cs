namespace Camagru.Domain.Entities;

public class Overlay
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; } // e.g., "Hearts", "Stars", "Emoji"
    public required string FilePath { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
