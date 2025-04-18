using EventManager.Application.Interfaces.Repositories;
using EventManager.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using EventManager.Application.Interfaces.Services;

namespace EventManager.Application.Services;

public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileService _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public ImageService(
        IImageRepository imageRepository,
        IFileService fileStorage,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _imageRepository = imageRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
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
            await _cacheService.RemoveEventImageAsync(eventId, filename);

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
        if (string.IsNullOrWhiteSpace(filename) ||
            filename.Contains("..") ||
            Path.IsPathRooted(filename))
        {
            throw new ArgumentException("Invalid filename");
        }

        var cachedImage = await _cacheService.GetEventImageAsync(eventId, filename);
        if (cachedImage != null)
        {
            return cachedImage;
        }

        var filePath = Path.Combine(
            Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads",
            filename);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Image not found", filename);
        }

        var imageBytes = await File.ReadAllBytesAsync(filePath);
        await _cacheService.SetEventImageAsync(eventId, filename, imageBytes);

        return imageBytes;
    }
}