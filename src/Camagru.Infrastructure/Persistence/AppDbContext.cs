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
            entity.HasMany(u => u.Images)
                .WithOne(i => i.User)
                .HasForeignKey(i => i.UserId)
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

        // Image entity configuration
        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.UserId)
                .IsRequired();

            entity.Property(i => i.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(i => i.CreatedAt)
                .IsRequired();

            // Relationships with cascade delete
            entity.HasOne(i => i.User)
                .WithMany(u => u.Images)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(i => i.Comments)
                .WithOne(c => c.Image)
                .HasForeignKey(c => c.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(i => i.Likes)
                .WithOne(l => l.Image)
                .HasForeignKey(l => l.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Comment entity configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.ImageId)
                .IsRequired();

            entity.Property(c => c.UserId)
                .IsRequired();

            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            // Relationships
            entity.HasOne(c => c.Image)
                .WithMany(i => i.Comments)
                .HasForeignKey(c => c.ImageId)
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

            entity.Property(l => l.ImageId)
                .IsRequired();

            entity.Property(l => l.UserId)
                .IsRequired();

            // Unique constraint: one like per user per image
            entity.HasIndex(l => new { l.ImageId, l.UserId })
                .IsUnique();

            // Relationships
            entity.HasOne(l => l.Image)
                .WithMany(i => i.Likes)
                .HasForeignKey(l => l.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
