using EventManager.Application.Requests;
using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface IEventRepository
{
    Task<PagedResponse<Event>> GetAllAsync(int pageNumber, int pageSize);

    Task<PagedResponse<Event>> GetFilteredAsync(EventFilterRequest filter,
                                                     int pageNumber,
                                                     int pageSize);

    Task AddAsync(Event newEvent);

    Task UpdateAsync(Event updatedEvent);

    Task<Event?> GetByIdAsync(Guid id);

    Task<Event?> GetByNameAsync(string name);

    Task DeleteAsync(Guid id);

    Task<PagedResponse<Event>> GetEventsByUserAsync(Guid userId, int pageNumber, int pageSize);



    Task<PagedResponse<Participant>> GetParticipantsAsync(Guid eventId,
                                                                int pageNumber,
                                                                 int pageSize);

    Task<bool> AnyEventWithCategoryAsync(Guid categoryId);
}


