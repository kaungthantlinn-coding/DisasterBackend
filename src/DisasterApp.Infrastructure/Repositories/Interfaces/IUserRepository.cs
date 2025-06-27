using DisasterApp.Domain.Entities;

namespace DisasterApp.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByAuthProviderAsync(string authProvider, string authId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsAsync(string email);
    Task<List<string>> GetUserRolesAsync(Guid userId);
}