using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface IImageRepository
{
    Task AddAsync(Image image, CancellationToken cst = default);
    Task UpdateAsync(Image image, CancellationToken cst = default);
    Task DeleteAsync(Image image, CancellationToken cst = default);
}