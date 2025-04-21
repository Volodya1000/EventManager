namespace EventManager.Domain.Interfaces.Repositories;

public interface IImageRepository
{
    public Task AddImageToEventAsync(Guid eventId, string imageUrl);

    public Task DeleteImageAsyncWithoutSaveChanges(Guid eventId, string imageUrl);

    public  Task<bool> ExistsAsync(Guid eventId, string url);
}