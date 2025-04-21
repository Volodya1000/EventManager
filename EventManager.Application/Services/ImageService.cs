using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Exceptions;

namespace EventManager.Application.Services;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IFileService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public ImageService(
        IImageRepository imageRepository,
        IEventRepository eventRepository,
        IFileService fileStorage,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _imageRepository = imageRepository;
        _eventRepository = eventRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }


    public async Task<string> UploadImageAsync(Guid eventId, IFormFile image)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new EventNotFoundException(eventId);

        string imageUrl = await _fileStorage.SaveFile(image);
        await _imageRepository.AddImageToEventAsync(eventId, imageUrl);
        return imageUrl;
    }

    public async Task DeleteImageAsync(Guid eventId, string filename)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new EventNotFoundException(eventId);

        var normalizedUrl = NormalizeUrl(filename);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (! await _imageRepository.ExistsAsync(eventId, normalizedUrl))
                throw new ArgumentException("Image not found");

            await _imageRepository.DeleteImageAsyncWithoutSaveChanges(eventId, normalizedUrl);
            await _fileStorage.DeleteFile(normalizedUrl);

            var fname = Path.GetFileName(normalizedUrl);
            await _cacheService.RemoveEventImageAsync(eventId, fname);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<(byte[] Bytes, string MimeType)> GetImageAsync(Guid eventId, string filename)
    {
        ValidateFilename(filename);

        var cachedImage = await _cacheService.GetEventImageAsync(eventId, filename);
        if (cachedImage != null)
            return (cachedImage, GetMimeType(filename));

        var filePath = Path.Combine(
            Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads",
            filename);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Image not found", filename);

        var imageBytes = await File.ReadAllBytesAsync(filePath);
        await _cacheService.SetEventImageAsync(eventId, filename, imageBytes);

        return (imageBytes, GetMimeType(filename));
    }

    #region Helpers

    private static void ValidateFilename(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename) ||
            filename.Contains("..") ||
            Path.IsPathRooted(filename))
        {
            throw new ArgumentException("Invalid file name ");
        }
    }

    private string NormalizeUrl(string filename)
    {
        if (filename.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return filename;
        return $"/uploads/{filename}";
    }

    private static string GetMimeType(string filename)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };
    }

    #endregion
}