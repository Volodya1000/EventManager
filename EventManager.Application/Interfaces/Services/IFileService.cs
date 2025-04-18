using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IFileService
{
    Task<string> SaveFile(IFormFile file);
    Task DeleteFile(string url);
}
