using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface IImageRepository
{
	Task<Image?> GetByIdAsync(int id);
	Task<IEnumerable<Image>> GetAllAsync();
	Task<IEnumerable<Image>> GetByPostIdAsync(int postId);
	Task AddAsync(Image image);
	Task UpdateAsync(Image image);
	Task DeleteAsync(Image image);
	Task<bool> ExistsAsync(int id);
}
