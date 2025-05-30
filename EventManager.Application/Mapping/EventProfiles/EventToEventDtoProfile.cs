﻿using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Domain.Models;

namespace EventManager.Application.Mapping.EventProfiles;

public class EventToEventDtoProfile : Profile
{
    public EventToEventDtoProfile()
    {
        CreateMap<Event, EventDto>()
            .ForMember(dest => dest.ImageUrls,
                       opt => opt.MapFrom(src => src.ImageUrls.ToList()))

            .ForMember(dest => dest.ParticipantsIds,
                       opt => opt.MapFrom(src => src.Participants
                           .Select(p => p.UserId)
                           .ToList()));
    }
}
