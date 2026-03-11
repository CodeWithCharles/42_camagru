using Microsoft.EntityFrameworkCore;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Infrastructure.Persistence.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly AppDbContext _context;

    public LikeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Like?> GetByIdAsync(int id)
    {
        return await _context.Likes.FindAsync(id);
    }

    public async Task<Like?> GetByImageAndUserAsync(int imageId, int userId)
    {
        return await _context.Likes
            .FirstOrDefaultAsync(l => l.ImageId == imageId && l.UserId == userId);
    }

    public async Task<IEnumerable<Like>> GetByImageIdAsync(int imageId)
    {
        return await _context.Likes
            .Include(l => l.User)
            .Where(l => l.ImageId == imageId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Like>> GetByUserIdAsync(int userId)
    {
        return await _context.Likes
            .Include(l => l.Image)
            .Where(l => l.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Like like)
    {
        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Like like)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int imageId, int userId)
    {
        return await _context.Likes
            .AnyAsync(l => l.ImageId == imageId && l.UserId == userId);
    }

    public async Task<int> GetCountByImageIdAsync(int imageId)
    {
        return await _context.Likes.CountAsync(l => l.ImageId == imageId);
    }
}
