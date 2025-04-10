using AutoMapper;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence.Repositories;

public class UserRepository:IUserRepository
{
    private readonly EventManagerDbContext _applicationDbContext;
    private readonly IMapper _mapper;

    public UserRepository(
        EventManagerDbContext applicationDbContext,
        IMapper mapper)
    {
        _applicationDbContext = applicationDbContext;
        _mapper = mapper;
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        var userEntity = await _applicationDbContext.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

        return userEntity == null
            ? null
            : _mapper.Map<User>(userEntity);
    }
}
