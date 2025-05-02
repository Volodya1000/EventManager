using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IParticipantRepository
{
    Task<PagedResponse<Participant>> GetParticipantsAsync(Guid eventId, int pageNumber, int pageSize, CancellationToken cst = default);
    Task AddAsync(Participant participant, CancellationToken cst = default);
    Task RemoveAsync(Participant participant, CancellationToken cst = default);
}