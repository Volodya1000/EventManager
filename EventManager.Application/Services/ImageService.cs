using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using EventManager.Application.Interfaces.Services;

namespace EventManager.Application.Services;

public class ImageService:IImageService
{
    private readonly IImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public ImageService(
        IImageRepository imageRepository,
        IFileStorage fileStorage,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _imageRepository=imageRepository;
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
            await _imageRepository.DeleteImageAsyncWithoutCommit(eventId, url); // Метод Без SaveChanges
            await _fileStorage.DeleteFile(url);
            await _unitOfWork.CommitAsync(); // Сохранение + коммит транзакции
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
