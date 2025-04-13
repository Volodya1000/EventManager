using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IImageService
{
    Task<string> UploadEventImageAsync(Guid eventId, IFormFile image);
    Task DeleteEventImageAsync(Guid eventId, string filename);
}

