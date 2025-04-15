﻿using EventManager.Application.Dtos;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IEventService
{
    Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize);
    Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize);
    Task<PagedResponse<EventDto>> GetEventsByUserAsync(Guid userId, int page, int pageSize);
    Task<EventDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateEventRequest request);
    Task UpdateAsync(Guid id, UpdateEventRequest request);
    Task DeleteAsync(Guid id);
    Task<Guid> RegisterAsync(Guid eventId,Guid userId);
    Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId,int pageNumber, int pageSize);
    Task CancelAsync(Guid eventId, Guid userId);
}


