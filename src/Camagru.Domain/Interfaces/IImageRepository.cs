using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface IImageRepository
{
	Task<Image?> GetByIdAsync(int id);
	Task<Image?> GetByIdWithDetailsAsync(int id);
	Task<IEnumerable<Image>> GetAllAsync();
	Task<IEnumerable<Image>> GetByUserIdAsync(int userId);
	Task<(IEnumerable<Image> Images, int TotalCount)> GetPagedGalleryAsync(int pageNumber, int pageSize);
	Task AddAsync(Image image);
	Task UpdateAsync(Image image);
	Task DeleteAsync(Image image);
	Task<bool> ExistsAsync(int id);
	Task<int> GetTotalCountAsync();
}
