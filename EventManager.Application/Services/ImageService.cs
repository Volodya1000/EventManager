using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using EventManager.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using EventManager.Application.Options;

namespace EventManager.Application.Services;

public class ImageService:IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;

    public ImageService(
        IImageRepository imageRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork,
        IDistributedCache cache)
    {
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _imageRepository = imageRepository;
        _cache = cache;
    }

    public async Task<string> UploadImageAsync(Guid eventId, IFormFile image)
    {
        string imageUrl = await _fileStorage.SaveFile(image);

        await _imageRepository.AddImageToEventAsync(eventId, imageUrl);
        return imageUrl;
    }

    public async Task DeleteImageAsync(Guid eventId, string url)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _imageRepository.DeleteImageAsyncWithoutCommit(eventId, url);
            await _fileStorage.DeleteFile(url);

            var filename = Path.GetFileName(url);
            var cacheKey = $"event_image:{eventId}:{filename}";
            await _cache.RemoveAsync(cacheKey);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<byte[]> GetImageAsync(Guid eventId, string filename)
    {
        // Валидация имени файла
        if (string.IsNullOrWhiteSpace(filename) ||
            filename.Contains("..") ||
            Path.IsPathRooted(filename))
        {
            throw new ArgumentException("Invalid filename");
        }

        var cacheKey = $"event_image:{eventId}:{filename}";
        var cachedImage = await _cache.GetAsync(cacheKey);

        if (cachedImage != null)
        {
            return cachedImage;
        }

        // Получение файла из хранилища
        var filePath = Path.Combine(
            Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads",
            filename);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Image not found", filename);
        }

        var imageBytes = await File.ReadAllBytesAsync(filePath);

        // Сохраняем в кэш
        await _cache.SetAsync(cacheKey, imageBytes, CacheOptions.DefaultExpiration);

        return imageBytes;
    }
}
