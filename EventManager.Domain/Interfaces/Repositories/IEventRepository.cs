using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IEventRepository
{
    #region OperationsWithEvents
    Task<PagedResponse<Event>> GetAllAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cst = default);

    Task<Event?> GetByIdAsync(
        Guid id,
        CancellationToken cst = default);

    Task<Event?> GetByNameAsync(
        string name,
        CancellationToken cst = default);

    Task AddAsync(
        Event newEvent,
        CancellationToken cst = default);

    Task UpdateAsync(
        Event updatedEvent,
        CancellationToken cst = default);

    Task DeleteAsync(
        Event deleteEvent,
        CancellationToken cst = default);

    Task<PagedResponse<Event>> GetFilteredAsync(
        int pageNumber,
        int pageSize,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? location = null,
        List<string>? categories = null,
        int? maxParticipants = null,
        int? availableSpaces = null,
        CancellationToken cst = default);

    Task<PagedResponse<Event>> GetEventsByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cst = default);

    Task<bool> AnyEventWithCategoryAsync(
        Guid categoryId,
        CancellationToken cst = default);
    #endregion

    #region OperationsWithParticipants
    Task<PagedResponse<Participant>> GetParticipantsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cst = default);
    #endregion
}


