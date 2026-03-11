using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface ILikeRepository
{
	Task<Like?> GetByIdAsync(int id);
	Task<Like?> GetByImageAndUserAsync(int imageId, int userId);
	Task<IEnumerable<Like>> GetByImageIdAsync(int imageId);
	Task<IEnumerable<Like>> GetByUserIdAsync(int userId);
	Task AddAsync(Like like);
	Task DeleteAsync(Like like);
	Task<bool> ExistsAsync(int imageId, int userId);
	Task<int> GetCountByImageIdAsync(int imageId);
}
