using EventManager.Application.Exceptions;
using EventManager.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

public class FileService : IFileService
{
    private readonly string _uploadPath;
    private static readonly string[] _allowedExtensions = { ".jpg", ".png", ".webp" };

    public FileService()
    {
        _uploadPath = Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads";
    }

    public async Task<string> SaveFile(IFormFile file, CancellationToken cst = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Invalid file type: {extension}");

        var originalName = Path.GetFileNameWithoutExtension(file.FileName);
        var sanitizedFileName = string.Concat(originalName
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
            .Trim();

        var uniqueName = $"{Guid.NewGuid()}_{sanitizedFileName}{extension}";
        var filePath = Path.Combine(_uploadPath, uniqueName);

        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cst);
            return $"/uploads/{uniqueName}";
        }
        catch (OperationCanceledException)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            throw;
        }
        catch (Exception ex)
        {
            throw new FileStorageException("File save failed", ex.Message);
        }
    }

    public async Task DeleteFile(string fileUrl, CancellationToken cst = default)
    {
        if (string.IsNullOrEmpty(fileUrl))
            throw new ArgumentNullException(nameof(fileUrl));

        var fileName = Path.GetFileName(fileUrl);
        var fullPath = Path.Combine(_uploadPath, fileName);

        try
        {
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cst);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FileStorageException("File deletion failed", ex.Message);
        }
    }
}