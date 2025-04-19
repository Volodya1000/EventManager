using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(Guid id, IFormFile image);
    Task DeleteImageAsync(Guid id, string url);
    Task<(byte[] Bytes, string MimeType)> GetImageAsync(Guid eventId, string filename);
}

