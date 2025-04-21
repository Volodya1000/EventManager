using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cst = default);
    Task<User?> GetUserById(Guid id, CancellationToken cst = default);
}
