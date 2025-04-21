using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Domain.Models;

namespace EventManager.Application.Mapping.PagedResponceProfiles;

public class PagedResponceProfile:Profile
{
    public PagedResponceProfile()
    {
        CreateMap<PagedResponse<Event>, PagedResponse<EventDto>>()
            .ConvertUsing<PagedResponseConverter<Event, EventDto>>();

        CreateMap<PagedResponse<Participant>, PagedResponse<ParticipantDto>>()
            .ConvertUsing<PagedResponseConverter<Participant, ParticipantDto>>();
    }
}
