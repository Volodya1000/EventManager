using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IFileService
{
    Task<string> SaveFile(IFormFile file, CancellationToken cst = default);
    Task DeleteFile(string fileUrl, CancellationToken cst = default);
}
