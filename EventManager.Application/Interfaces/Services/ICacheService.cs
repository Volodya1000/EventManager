namespace EventManager.Application.Interfaces.Services;

public interface ICacheService
{
    Task<byte[]> GetEventImageAsync(
        Guid eventId,
        string filename,
        CancellationToken cst = default);

    Task SetEventImageAsync(
        Guid eventId,
        string filename,
        byte[] imageBytes,
        CancellationToken cst = default);

    Task RemoveEventImageAsync(
        Guid eventId,
        string filename,
        CancellationToken cst = default);
}