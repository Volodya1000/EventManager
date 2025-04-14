using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<EventEntity, EventDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.RegisteredParticipants, opt => opt.MapFrom(src => src.Participants.Count))
            .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.Select(i => i.Url)))
            .ForMember(dest => dest.ParticipantsIds, opt => opt.MapFrom(src => src.Participants.Select(p => p.UserId)));
        

        CreateMap<EventEntity, Event>()
                   .ConstructUsing(src => Event.Create(
                       src.Id,
                       src.Name,
                       src.Description,
                       src.DateTime,
                       src.Location,
                       src.Category.Name, 
                       src.MaxParticipants,
                       src.Images.Select(i => i.Url).ToList()))
                   .ForMember(dest => dest.Participants, opt => opt.Ignore())// Игнорируем автоматический маппинг
                   .AfterMap((src, dest) =>
                   {
                       // Маппинг участников
                       foreach (var participantEntity in src.Participants)
                       {
                           dest.AddParticipant(Participant.Create(
                               participantEntity.UserId,
                               participantEntity.EventId,
                               participantEntity.RegistrationDate,
                               participantEntity.User.FirstName,
                               participantEntity.User.LastName,
                               participantEntity.User.DateOfBirth));
                       }
                   });
    }
}
