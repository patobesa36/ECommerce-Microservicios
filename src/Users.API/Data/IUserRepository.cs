using Users.API.Models;

namespace Users.API.Data;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateLoginStateAsync(Guid id, bool activo, int intentosFallidos);
}