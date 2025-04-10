using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
}
