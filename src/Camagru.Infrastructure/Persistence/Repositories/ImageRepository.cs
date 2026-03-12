using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

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

	public async Task<IEnumerable<Image>> GetAllAsync()
	{
		return await _context.Images.ToListAsync();
	}

	public async Task<IEnumerable<Image>> GetByPostIdAsync(int postId)
	{
		return await _context.Images
			.Where(i => i.PostId == postId)
			.OrderBy(i => i.Id)
			.ToListAsync();
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
}
