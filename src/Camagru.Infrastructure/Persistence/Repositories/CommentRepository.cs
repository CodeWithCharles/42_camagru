using Camagru.Domain.Entities;
using Camagru.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Camagru.Infrastructure.Persistence.Repositories;

public class CommentRepository : ICommentRepository
{
	private readonly AppDbContext _context;

	public CommentRepository(AppDbContext context)
	{
		_context = context;
	}

	public async Task<Comment?> GetByIdAsync(int id)
	{
		return await _context.Comments
			.Include(c => c.User)
			.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
	{
		return await _context.Comments
			.Include(c => c.User)
			.Where(c => c.PostId == postId)
			.OrderBy(c => c.CreatedAt)
			.ToListAsync();
	}

	public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
	{
		return await _context.Comments
			.Include(c => c.Post)
			.Where(c => c.UserId == userId)
			.OrderByDescending(c => c.CreatedAt)
			.ToListAsync();
	}

	public async Task AddAsync(Comment comment)
	{
		await _context.Comments.AddAsync(comment);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateAsync(Comment comment)
	{
		_context.Comments.Update(comment);
		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(Comment comment)
	{
		_context.Comments.Remove(comment);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> ExistsAsync(int id)
	{
		return await _context.Comments.AnyAsync(c => c.Id == id);
	}

	public async Task<int> GetCountByPostIdAsync(int postId)
	{
		return await _context.Comments.CountAsync(c => c.PostId == postId);
	}
}
