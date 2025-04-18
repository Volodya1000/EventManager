namespace EventManager.Application.Interfaces.Services;

public interface ICacheService
{
    Task<byte[]> GetEventImageAsync(Guid eventId, string filename);
    Task SetEventImageAsync(Guid eventId, string filename, byte[] imageBytes);
    Task RemoveEventImageAsync(Guid eventId, string filename);
}