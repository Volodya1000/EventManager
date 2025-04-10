using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping;

internal class UserMappingProfile : Profile
{
    //public UserMappingProfile()
    //{
    //    CreateMap<UserEntity, User>()
    //        .ConvertUsing(src => new User(
    //            src.Id,
    //            src.FirstName,
    //            src.LastName,
    //            src.DateOfBirth,
    //            src.Email,
    //            src.PasswordHash,
    //            src.RefreshToken,
    //            src.RefreshTokenExpiresAtUtc
    //        ));
    //}
}

