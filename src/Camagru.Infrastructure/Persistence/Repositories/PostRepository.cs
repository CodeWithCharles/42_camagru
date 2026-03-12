using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Camagru.Infrastructure.Persistence.Repositories;

public class PostRepository : IPostRepository
{
	private readonly AppDbContext _context;

	public PostRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<Post?> GetByIdAsync(int id)
	{
		return await _context.Posts.FindAsync(id);
	}

	public async Task<Post?> GetByIdWithDetailsAsync(int id)
	{
		return await _context.Posts
			.Include(p => p.User)
			.Include(p => p.Images)
			.Include(p => p.Comments)
				.ThenInclude(c => c.User)
			.Include(p => p.Likes)
				.ThenInclude(l => l.User)
			.FirstOrDefaultAsync(p => p.Id == id);
	}

	public async Task<IEnumerable<Post>> GetAllAsync()
	{
		return await _context.Posts
			.Include(p => p.User)
			.Include(p => p.Images)
			.OrderByDescending(p => p.Id)
			.ToListAsync();
	}

	public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
	{
		return await _context.Posts
			.Include(p => p.Images)
			.Where(p => p.UserId == userId)
			.OrderByDescending(p => p.Id)
			.ToListAsync();
	}

	public async Task<(IEnumerable<Post> Posts, int TotalCount)> GetPagedGalleryAsync(int pageNumber, int pageSize)
	{
		var totalCount = await _context.Posts.CountAsync();
		
		var posts = await _context.Posts
			.Include(p => p.User)
			.Include(p => p.Images)
			.Include(p => p.Comments)
			.Include(p => p.Likes)
			.OrderByDescending(p => p.Id)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return (posts, totalCount);
	}

	public async Task AddAsync(Post post)
	{
		await _context.Posts.AddAsync(post);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(Post post)
	{
		_context.Posts.Update(post);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Post post)
	{
		_context.Posts.Remove(post);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> ExistsAsync(int id)
	{
		return await _context.Posts.AnyAsync(p => p.Id == id);
	}

	public async Task<int> GetTotalCountAsync()
	{
		return await _context.Posts.CountAsync();
	}
}
