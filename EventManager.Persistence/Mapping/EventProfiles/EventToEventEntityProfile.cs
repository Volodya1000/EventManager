using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.EventProfiles;

public class EventToEventEntityProfile:Profile
{
    public EventToEventEntityProfile() 
    {
        CreateMap<Event, EventEntity>()
            .ForMember(dest => dest.Participants, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());
    }
}
