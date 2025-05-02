using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.EventProfiles;

public class EventToEventEntityProfile:Profile
{
    public EventToEventEntityProfile() 
    {
        CreateMap<Event, EventEntity>()
           .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
           .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
               src.ImageUrls.Select(url => new ImageEntity { Url = url })))
           .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.Participants));
    }
}
