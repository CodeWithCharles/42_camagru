using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Camagru.Infrastructure.Persistence.Repositories;

public class OverlayRepository : IOverlayRepository
{
    private readonly AppDbContext _context;

    public OverlayRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Overlay?> GetByIdAsync(int id)
    {
        return await _context.Overlays.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Overlay>> GetAllAsync()
    {
        return await _context.Overlays.OrderBy(o => o.DisplayOrder).ToListAsync();
    }

    public async Task<List<Overlay>> GetActiveAsync()
    {
        return await _context.Overlays.OrderBy(o => o.DisplayOrder).ToListAsync();
    }

    public async Task AddAsync(Overlay overlay)
    {
        await _context.Overlays.AddAsync(overlay);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Overlay overlay)
    {
        _context.Overlays.Update(overlay);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Overlay overlay)
    {
        _context.Overlays.Remove(overlay);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Overlays.AnyAsync(o => o.Id == id);
    }
}
