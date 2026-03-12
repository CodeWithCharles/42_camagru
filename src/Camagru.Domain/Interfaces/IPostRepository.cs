using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface IPostRepository
{
	Task<Post?> GetByIdAsync(int id);
	Task<Post?> GetByIdWithDetailsAsync(int id);
	Task<IEnumerable<Post>> GetAllAsync();
	Task<IEnumerable<Post>> GetByUserIdAsync(int userId);
	Task<(IEnumerable<Post> Posts, int TotalCount)> GetPagedGalleryAsync(int pageNumber, int pageSize);
	Task AddAsync(Post post);
	Task UpdateAsync(Post post);
	Task DeleteAsync(Post post);
	Task<bool> ExistsAsync(int id);
	Task<int> GetTotalCountAsync();
}
