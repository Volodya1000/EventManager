using EventManager.Application.Interfaces.Repositories;
using EventManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public UserRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _applicationDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _applicationDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

    }
}
