using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.ParticipantProfiles;


public class ParticipantToParticipantEntityProfile : Profile
{
    public ParticipantToParticipantEntityProfile()
    {
        CreateMap<Participant, ParticipantEntity>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Event, opt => opt.Ignore());
    }
}