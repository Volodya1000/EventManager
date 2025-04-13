using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EventManager.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    private readonly IMapper _mapper;

    private readonly IFileStorage _fileStorage;

    public EventService(IEventRepository eventRepository,
                        IMapper mapper,
                        IFileStorage fileStorage)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    public async Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize)
    {
        return await _eventRepository.GetAllAsync(page, pageSize);
    }

    public async Task<Guid> CreateAsync(CreateEventRequest request)
    {
        var newEvent = Event.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.DateTime,
            request.Location,
            request.Category,
            request.MaxParticipants,
            request.ImageUrls.ToList());

        await _eventRepository.AddAsync(newEvent);

        return newEvent.Id; 
    }


    public async Task CancelAsync(Guid eventId, Guid userId)
    {
        throw new NotImplementedException();
    }

    

    public async Task DeleteAsync(Guid id)
    {
        await _eventRepository.DeleteAsync(id);
    }  

    public async Task<EventDto?> GetByIdAsync(Guid eventId)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId);

        return _mapper.Map<EventDto>(eventById);
    }

    public async Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize)
    {
        return await _eventRepository.GetFilteredAsync(filterRequest, page, pageSize);
    }

    public async Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> RegisterAsync(Guid eventId, RegisterParticipantRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Guid eventId, UpdateEventRequest request)
    {
        Event eventById = await _eventRepository.GetByIdAsync(eventId);

        eventById.UpdateDescription(request.Description);

        eventById.UpdateLocation(request.Location);

        eventById.UpdateMaxParticipants(request.MaxParticipants);


        await _eventRepository.UpdateAsync(eventById);
    }


    public  async Task<string> UploadImageAsync(Guid eventId, IFormFile image)
    {
        string imageUrl = await _fileStorage.SaveFile(image);

        await _eventRepository.AddImageToEventAsync(eventId, imageUrl);

        return imageUrl;
    }

    public async Task DeleteImageAsync(Guid eventId, string url)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId);

        if (eventById == null) throw new InvalidOperationException($"Event with id: {eventId} not found");


        await _fileStorage.DeleteFile(url);

        await _eventRepository.DeleteImageAsync(eventId, url);
    }
}
