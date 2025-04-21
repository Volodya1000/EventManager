using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IEventRepository
{
    Task<PagedResponse<Event>> GetAllAsync(int pageNumber, int pageSize);

    Task<PagedResponse<Event>> GetFilteredAsync(
         int pageNumber,
         int pageSize,
         DateTime? dateFrom = null,
         DateTime? dateTo = null,
         string? location = null,
         List<string>? categories = null,
         int? maxParticipants = null,
         int? availableSpaces = null);

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


