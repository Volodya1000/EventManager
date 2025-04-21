using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

    Task<User?> GetUserById(Guid id);
}
