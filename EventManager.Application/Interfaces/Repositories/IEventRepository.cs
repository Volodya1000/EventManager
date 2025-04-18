using EventManager.Application.Dtos;
using EventManager.Application.Requests;
using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface IEventRepository
{
    Task<PagedResponse<EventDto>> GetAllAsync(int pageNumber, int pageSize);

    Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filter,
                                                     int pageNumber,
                                                     int pageSize);

    Task AddAsync(Event newEvent);

    Task UpdateAsync(Event updatedEvent);

    Task<Event?> GetByIdAsync(Guid id);

    Task<Event?> GetByNameAsync(string name);

    Task DeleteAsync(Guid id);

    Task<PagedResponse<EventDto>> GetEventsByUserAsync(Guid userId, int pageNumber, int pageSize);



    Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId,
                                                                int pageNumber,
                                                                 int pageSize);

    Task<bool> AnyEventWithCategoryAsync(Guid categoryId);
}


