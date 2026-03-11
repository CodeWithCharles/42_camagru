using Camagru.Domain.Entities;

namespace Camagru.Domain.Interfaces;

public interface IUserRepository
{
	Task<User?> GetByIdAsync(int id);
	Task<User?> GetByEmailAsync(string email);
	Task<User?> GetByUsernameAsync(string username);
	Task<User?> GetByConfirmationTokenAsync(string token);
	Task<User?> GetByResetTokenAsync(string token);
	Task<IEnumerable<User>> GetAllAsync();
	Task AddAsync(User user);
	Task UpdateAsync(User user);
	Task DeleteAsync(User user);
	Task<bool> ExistsAsync(int id);
	Task<bool> EmailExistsAsync(string email);
	Task<bool> UsernameExistsAsync(string username);
}
