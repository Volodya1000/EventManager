using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IImageRepository
{
    Task AddAsync(Image image, CancellationToken cst = default);
    Task UpdateAsync(Image image, CancellationToken cst = default);
    Task DeleteImageAsyncWithoutSaveChanges(Image image, CancellationToken cst = default);
    Task<bool> ExistsAsync(Guid eventId, string url, CancellationToken cst);
}