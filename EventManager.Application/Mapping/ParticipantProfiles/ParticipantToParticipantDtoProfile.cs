using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Domain.Models;

namespace EventManager.Application.Mapping.ParticipantProfiles;

public class ParticipantToParticipantDtoProfile:Profile
{
    public ParticipantToParticipantDtoProfile() 
    {
        CreateMap<Participant, ParticipantDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId))
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegistrationDate))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName));
       
    }
}
