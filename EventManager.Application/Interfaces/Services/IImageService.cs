using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(
        Guid eventId,
        IFormFile image,
        CancellationToken cst = default);

    Task DeleteImageAsync(
        Guid eventId,
        string filename,
        CancellationToken cst = default);

    Task<(byte[] Bytes, string MimeType)> GetImageAsync(
        Guid eventId,
        string filename,
        CancellationToken cst = default);
}

