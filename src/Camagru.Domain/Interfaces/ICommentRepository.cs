using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface ICommentRepository
{
	Task<Comment?> GetByIdAsync(int id);
	Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
	Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);
	Task AddAsync(Comment comment);
	Task UpdateAsync(Comment comment);
	Task DeleteAsync(Comment comment);
	Task<bool> ExistsAsync(int id);
	Task<int> GetCountByPostIdAsync(int postId);
}
