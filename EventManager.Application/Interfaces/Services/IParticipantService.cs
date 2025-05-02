using EventManager.Application.Dtos;
using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Services;

public interface IParticipantService
{
    Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId, int pageNumber, int pageSize, CancellationToken cst = default);
    Task<Guid> RegisterAsync(Guid eventId, CancellationToken cst);
    Task CancelAsync(Guid eventId, CancellationToken cst = default);
}