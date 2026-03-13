using Microsoft.EntityFrameworkCore;
using Camagru.Domain.Entities;

namespace Camagru.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Overlay> Overlays => Set<Overlay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.IsConfirmed)
                .IsRequired();

            entity.Property(u => u.ConfirmationToken)
                .HasMaxLength(255);

            entity.Property(u => u.ResetToken)
                .HasMaxLength(255);

            entity.Property(u => u.ResetTokenExpiry);

            entity.Property(u => u.EmailNotificationsEnabled)
                .IsRequired();

            entity.Property(u => u.DisplayName)
                .HasMaxLength(30);

            entity.Property(u => u.Bio)
                .HasMaxLength(500);

            entity.Property(u => u.AvatarImageId);

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            // Unique constraints
            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.HasIndex(u => u.ConfirmationToken)
                .IsUnique()
                .HasFilter("\"ConfirmationToken\" IS NOT NULL");

            entity.HasIndex(u => u.ResetToken)
                .IsUnique()
                .HasFilter("\"ResetToken\" IS NOT NULL");

            // Relationships with cascade delete
            entity.HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Post entity configuration
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.UserId)
                .IsRequired();

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            // Relationships with cascade delete
            entity.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Images)
                .WithOne(i => i.Post)
                .HasForeignKey(i => i.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Image entity configuration
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.PostId)
                .IsRequired();

            entity.Property(i => i.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(i => i.DisplayOrder)
                .IsRequired();

            entity.Property(i => i.CreatedAt)
                .IsRequired();

            // Relationships with cascade delete
            entity.HasOne(i => i.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Comment entity configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.PostId)
                .IsRequired();

            entity.Property(c => c.UserId)
                .IsRequired();

            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            // Relationships
            entity.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Like entity configuration
        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.PostId)
                .IsRequired();

            entity.Property(l => l.UserId)
                .IsRequired();

            // Unique constraint: one like per user per post
            entity.HasIndex(l => new { l.PostId, l.UserId })
                .IsUnique();

            // Relationships
            entity.HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Overlay entity configuration
        modelBuilder.Entity<Overlay>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(o => o.Category)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(o => o.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(o => o.DisplayOrder)
                .IsRequired();

            entity.Property(o => o.CreatedAt)
                .IsRequired();
        });
    }
}
