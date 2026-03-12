using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Camagru.Infrastructure.Persistence.Repositories;

public class LikeRepository : ILikeRepository
{
	private readonly AppDbContext _context;

	public LikeRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<Like?> GetByIdAsync(int id)
	{
		return await _context.Likes.FindAsync(id);
	}

	public async Task<Like?> GetByPostAndUserAsync(int postId, int userId)
	{
		return await _context.Likes
			.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
	}

	public async Task<IEnumerable<Like>> GetByPostIdAsync(int postId)
	{
		return await _context.Likes
			.Include(l => l.User)
			.Where(l => l.PostId == postId)
			.ToListAsync();
	}

	public async Task<IEnumerable<Like>> GetByUserIdAsync(int userId)
	{
		return await _context.Likes
			.Include(l => l.Post)
			.Where(l => l.UserId == userId)
			.ToListAsync();
	}

	public async Task AddAsync(Like like)
	{
		await _context.Likes.AddAsync(like);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Like like)
	{
		_context.Likes.Remove(like);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> ExistsAsync(int postId, int userId)
	{
		return await _context.Likes
			.AnyAsync(l => l.PostId == postId && l.UserId == userId);
	}

	public async Task<int> GetCountByPostIdAsync(int postId)
	{
		return await _context.Likes.CountAsync(l => l.PostId == postId);
	}
}
