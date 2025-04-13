using EventManager.Application.Dtos;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IEventService
{
    Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize);
    Task<List<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize);
    Task<EventDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreateEventRequest request);
    Task UpdateAsync(int id, UpdateEventRequest request);
    Task DeleteAsync(int id);
    Task<string> UploadImageAsync(int id, IFormFile image);
    Task<int> RegisterAsync(int eventId, RegisterParticipantRequest request);
    Task<List<ParticipantDto>> GetParticipantsAsync(int eventId);
    Task CancelAsync(int eventId, int userId);
}

