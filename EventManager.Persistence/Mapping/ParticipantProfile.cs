using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping;


public class ParticipantProfile : Profile
{
    public ParticipantProfile()
    {
        CreateMap<ParticipantEntity, ParticipantDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth));
    }
}