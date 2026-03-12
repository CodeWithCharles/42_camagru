using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface ILikeRepository
{
	Task<Like?> GetByIdAsync(int id);
	Task<Like?> GetByPostAndUserAsync(int postId, int userId);
	Task<IEnumerable<Like>> GetByPostIdAsync(int postId);
	Task<IEnumerable<Like>> GetByUserIdAsync(int userId);
	Task AddAsync(Like like);
	Task DeleteAsync(Like like);
	Task<bool> ExistsAsync(int postId, int userId);
	Task<int> GetCountByPostIdAsync(int postId);
}
