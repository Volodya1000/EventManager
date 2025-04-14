namespace EventManager.Application.Interfaces.Repositories;

public interface IImageRepository
{
    public Task AddImageToEventAsync(Guid eventId, string imageUrl);

    public Task DeleteImageAsyncWithoutCommit(Guid eventId, string imageUrl);
}