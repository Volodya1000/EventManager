using EventManager.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace EventManager.Application.FileStorage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _uploadPath;
    private static readonly string[] _allowedExtensions = { ".jpg", ".png", ".webp" };

    public LocalFileStorage()
    {
        // Путь берется из переменной окружения или использует дефолт
        _uploadPath = Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads";
    }

    public async Task<string> SaveFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Invalid file type: {extension}");

        // Очищаем имя файла от небезопасных символов и удаляем расширение
        var originalName = Path.GetFileNameWithoutExtension(file.FileName);
        var sanitizedFileName = string.Concat(originalName
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
            .Trim();

        var uniqueName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}";
        var filePath = Path.Combine(_uploadPath, uniqueName);

        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{uniqueName}";
        }
        catch (Exception ex)
        {
            throw new FileStorageException("File save failed", ex.Message);
        }
    }

    public Task DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            throw new ArgumentNullException(nameof(fileUrl));

        var fileName = Path.GetFileName(fileUrl);
        var fullPath = Path.Combine(_uploadPath, fileName);

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new FileStorageException("File deletion failed", ex.Message);
        }
    }
}
