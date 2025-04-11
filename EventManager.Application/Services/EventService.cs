using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Requests;
using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Services;

public class EventService : IEventService
{
    public Task CancelAsync(int eventId, int userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateAsync(CreateEventRequest request)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<EventDto>> GetAllAsync(int page, int pageSize)
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
