using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IFileStorage _fileStorage;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateEventRequest> _validator;

    public EventService(
        IEventRepository eventRepository,
        IMapper mapper,
        IFileStorage fileStorage,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateEventRequest> validator)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _fileStorage = fileStorage;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }


    #region OperationsWithEvents
    public async Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize)
    {
        return await _eventRepository.GetAllAsync(page, pageSize);
    }

    public async Task<Guid> CreateAsync(CreateEventRequest request)
    {
        _validator.ValidateAndThrow(request);

        var exists = await _eventRepository.GetByNameAsync(request.Name);
        if (exists != null)
            throw new InvalidOperationException("Event with this name already exists");

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

    public async Task DeleteAsync(Guid id)
    {
        await _eventRepository.DeleteAsync(id);
    }  

    public async Task<EventDto?> GetByIdAsync(Guid eventId)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId);

        return _mapper.Map<EventDto>(eventById);
    }

    public async Task UpdateAsync(Guid eventId, UpdateEventRequest request)
    {
        Event eventById = await _eventRepository.GetByIdAsync(eventId);

        eventById.UpdateDescription(request.Description);

        eventById.UpdateLocation(request.Location);

        eventById.UpdateMaxParticipants(request.MaxParticipants);


        await _eventRepository.UpdateAsync(eventById);
    }
    #endregion

    #region OperationsWithParticipants
    public async Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize)
    {
        return await _eventRepository.GetFilteredAsync(filterRequest, page, pageSize);
    }

    public async Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId,int pageNumber,int pageSize)
    {
        return await _eventRepository.GetParticipantsAsync(eventId, pageNumber, pageSize);
    }

    public async Task<Guid> RegisterAsync(Guid eventId, Guid userId)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId);

        if (eventById == null) 
            throw new InvalidOperationException($"Event with id: {eventId} not found");

        var user = await _userRepository.GetUserById(userId);

        if (user == null) 
            throw new InvalidOperationException($"User with id: {userId} not found");

        var newParticipant = Participant.Create(
            userId,
            eventId,
            DateTime.Now,
            user.FirstName,
            user.LastName,
            user.DateOfBirth);

        eventById.AddParticipant(newParticipant);

        await _eventRepository.UpdateAsync(eventById);

        return userId;
    }

    public async Task CancelAsync(Guid eventId, Guid userId)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId);

        if (eventById == null)
            throw new InvalidOperationException($"Event with id: {eventId} not found");

        var user = await _userRepository.GetUserById(userId);

        if (user == null)
            throw new InvalidOperationException($"User with id: {userId} not found");

        eventById.RemoveParticipant(userId);

        await _eventRepository.UpdateAsync(eventById);
    }


    #endregion
}



