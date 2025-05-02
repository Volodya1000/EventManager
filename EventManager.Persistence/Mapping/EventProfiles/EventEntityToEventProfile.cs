using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.EventProfiles;

public class EventEntityToEventProfile : Profile
{
    public EventEntityToEventProfile()
    {
        CreateMap<EventEntity, Event>()
                   .ConstructUsing(src => Event.Create(
                       src.Id,
                       src.Name,
                       src.Description,
                       src.DateTime,
                       src.Location,
                       src.Category.Id,
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
