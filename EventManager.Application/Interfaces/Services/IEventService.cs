using EventManager.Application.Dtos;
using EventManager.Application.Requests;
using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Services;

public interface IEventService
{
    Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize, CancellationToken cst = default);
    Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize, CancellationToken cst = default);
    Task<PagedResponse<EventDto>> GetEventsByUserAsync(int page, int pageSize, CancellationToken cst = default);
    Task<EventDto> GetByIdAsync(Guid id, CancellationToken cst = default);
    Task<Guid> CreateAsync(CreateEventRequest request, CancellationToken cst = default);
    Task UpdateAsync(Guid id, UpdateEventRequest request, CancellationToken cst = default);
    Task DeleteAsync(Guid id, CancellationToken cst = default);
    Task<Guid> RegisterAsync(Guid eventId, CancellationToken cst = default);
    Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId, int pageNumber, int pageSize, CancellationToken cst = default);
    Task CancelAsync(Guid eventId, CancellationToken cst = default);
}
