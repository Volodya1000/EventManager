using AutoMapper;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using System;

namespace EventManager.Application.Services;

public class ImageService : IImageService
{
    private readonly IEventRepository _eventRepository;
    private readonly IFileStorage _fileStorage;

    public ImageService(
        IEventRepository eventRepository,
        IFileStorage fileStorage)
    {
        _eventRepository = eventRepository;
        _fileStorage = fileStorage;
    }
    

    public async Task<string> UploadEventImageAsync(Guid eventId, IFormFile image)
    {
        string imageUrl = await _fileStorage.SaveFile(image);

        await _eventRepository.AddImageToEventAsync(eventId, imageUrl);

        return imageUrl;
    }

    public async Task DeleteEventImageAsync(Guid eventId, string url)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId);

        if (eventById == null) throw new InvalidOperationException($"Event with id: {eventId} not found");


        await _fileStorage.DeleteFile(url);

        await _eventRepository.DeleteImageAsync(eventId, url);
    }
}
