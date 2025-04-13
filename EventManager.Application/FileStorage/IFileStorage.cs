using Microsoft.AspNetCore.Http;

namespace EventManager.Application.FileStorage;

public interface IFileStorage
{
    Task<string> SaveFile(IFormFile file);
    Task DeleteFile(string url);
}
