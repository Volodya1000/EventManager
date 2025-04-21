namespace EventManager.Domain.Interfaces.Repositories;

public interface IImageRepository
{
    Task AddImageToEventAsync(
        Guid eventId, 
        string imageUrl, 
        CancellationToken cst = default);
        
    Task DeleteImageAsyncWithoutSaveChanges(
        Guid eventId, 
        string imageUrl, 
        CancellationToken cst = default);
        
    Task<bool> ExistsAsync(
        Guid eventId, 
        string url, 
        CancellationToken cst = default);
}