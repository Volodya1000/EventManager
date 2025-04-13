using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize)
    {
        return await _eventRepository.GetAllAsync(page, pageSize);
    }

    public async Task<Guid> CreateAsync(CreateEventRequest request)
    {
        var newEvent = Event.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.DateTime,
            request.Location,
            request.Category,
            request.MaxParticipants,
            request.ImageUrls.ToList());

        await _eventRepository.AddAsync(newEvent);

        return newEvent.Id; 
    }


    public Task CancelAsync(int eventId, int userId)
    {
        throw new NotImplementedException();
    }

    

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }  

    public Task<EventDto?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<List<ParticipantDto>> GetParticipantsAsync(int eventId)
    {
        throw new NotImplementedException();
    }

    public Task<int> RegisterAsync(int eventId, RegisterParticipantRequest request)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(int id, UpdateEventRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<string> UploadImageAsync(int id, IFormFile image)
    {
        throw new NotImplementedException();
    }
}
