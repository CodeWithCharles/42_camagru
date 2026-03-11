using Microsoft.EntityFrameworkCore;
using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;

namespace Camagru.Infrastructure.Persistence.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _context;

    public ImageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Image?> GetByIdAsync(int id)
    {
        return await _context.Images.FindAsync(id);
    }

    public async Task<Image?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Images
            .Include(i => i.User)
            .Include(i => i.Comments)
                .ThenInclude(c => c.User)
            .Include(i => i.Likes)
                .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Image>> GetAllAsync()
    {
        return await _context.Images
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Image>> GetByUserIdAsync(int userId)
    {
        return await _context.Images
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Image> Images, int TotalCount)> GetPagedGalleryAsync(int pageNumber, int pageSize)
    {
        var query = _context.Images
            .Include(i => i.User)
            .OrderByDescending(i => i.CreatedAt);

        var totalCount = await query.CountAsync();
        
        var images = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (images, totalCount);
    }

    public async Task AddAsync(Image image)
    {
        await _context.Images.AddAsync(image);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Image image)
    {
        _context.Images.Update(image);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Image image)
    {
        _context.Images.Remove(image);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Images.AnyAsync(i => i.Id == id);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Images.CountAsync();
    }
}
