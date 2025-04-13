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
        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

        return user;
    }
}
