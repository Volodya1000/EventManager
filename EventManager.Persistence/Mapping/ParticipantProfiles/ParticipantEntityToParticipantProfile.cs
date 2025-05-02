using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.ParticipantProfiles;

public class ParticipantEntityToParticipantProfile : Profile
{
    public ParticipantEntityToParticipantProfile()
    {
        CreateMap<ParticipantEntity, Participant>()
            .ConstructUsing(src => Participant.Create(
                src.UserId,
                src.EventId,
                src.RegistrationDate,
                src.User.FirstName,
                src.User.LastName,
                src.User.DateOfBirth))
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore());
    }
}
