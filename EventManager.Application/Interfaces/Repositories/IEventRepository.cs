using EventManager.Application.Dtos;
using EventManager.Application.Requests;
using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface IEventRepository
{
    public Task<PagedResponse<EventDto>> GetAllAsync(int pageNumber, int pageSize);

    public Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filter,
                                                     int pageNumber,
                                                     int pageSize);

    public  Task AddAsync(Event newEvent);

    public Task UpdateAsync(Event updatedEvent);

    public  Task<Event?> GetByIdAsync(Guid id);

    public Task<Event?> GetByNameAsync(string name);

    public Task DeleteAsync(Guid id);

    public Task<PagedResponse<EventDto>> GetEventsByUserAsync(Guid userId, int pageNumber, int pageSize);



    public Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId,
                                                                int pageNumber,
                                                                 int pageSize);
}


