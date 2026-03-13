using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface IOverlayRepository
{
    Task<Overlay?> GetByIdAsync(int id);
    Task<List<Overlay>> GetAllAsync();
    Task<List<Overlay>> GetActiveAsync();
    Task AddAsync(Overlay overlay);
    Task UpdateAsync(Overlay overlay);
    Task DeleteAsync(Overlay overlay);
    Task<bool> ExistsAsync(int id);
}
