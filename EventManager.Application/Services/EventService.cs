using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using EventManager.Application.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EventManager.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IAccountService _accountService;
    private readonly IValidator<CreateEventRequest> _createValidator;
    private readonly IValidator<UpdateEventRequest> _updateValidator;
    private readonly IValidator<EventFilterRequest> _filterValidator;

    public EventService(
        IEventRepository eventRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IUserRepository userRepository,
        IAccountService accountService,
        IValidator<CreateEventRequest> createValidator,
        IValidator<UpdateEventRequest> updateValidator,
        IValidator<EventFilterRequest> filterValidator)
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _userRepository = userRepository;
        _accountService = accountService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    public async Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize, CancellationToken cst = default)
    {
        var result = await _eventRepository.GetAllAsync(page, pageSize, cst);
        return _mapper.Map<PagedResponse<EventDto>>(result);
    }

    public async Task<Guid> CreateAsync(CreateEventRequest request, CancellationToken cst = default)
    {
        _createValidator.ValidateAndThrow(request);
        var exists = await _eventRepository.GetByNameAsync(request.Name, cst);
        if (exists != null)
            throw new InvalidOperationException("Event with this name already exists");

        var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId, cst)
            ??throw new NotFoundException($"Category with id {request.CategoryId} not found");

        var newEvent = Event.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description,
            request.DateTime,
            request.Location,
            request.CategoryId,
            request.MaxParticipants);

        await _eventRepository.AddAsync(newEvent, cst);
        return newEvent.Id;
    }

    public async Task DeleteAsync(Guid eventId, CancellationToken cst = default)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId, cst)
          ?? throw new NotFoundException($"Event with id {eventId} not found");

        await _eventRepository.DeleteAsync(eventById, cst);
    }

    public async Task<EventDto> GetByIdAsync(Guid eventId, CancellationToken cst = default)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId, cst)
          ?? throw new NotFoundException($"Event with id {eventId} not found");
        return _mapper.Map<EventDto>(eventById);
    }

    public async Task UpdateAsync(Guid eventId, UpdateEventRequest request, CancellationToken cst = default)
    {
        _updateValidator.ValidateAndThrow(request);
        var eventById = await _eventRepository.GetByIdAsync(eventId, cst)
           ?? throw new NotFoundException($"Event with id {eventId} not found");

        eventById.UpdateDescription(request.Description);
        eventById.UpdateLocation(request.Location);
        eventById.UpdateMaxParticipants(request.MaxParticipants);

        await _eventRepository.UpdateAsync(eventById, cst);
    }

    public async Task<PagedResponse<EventDto>> GetEventsByUserAsync(int page, int pageSize, CancellationToken cst = default)
    {
        var userId = _accountService.GetCurrentUserId();
        var result = await _eventRepository.GetEventsByUserAsync(userId, page, pageSize, cst);
        return _mapper.Map<PagedResponse<EventDto>>(result);
    }

    public async Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize, CancellationToken cst = default)
    {
        _filterValidator.ValidateAndThrow(filterRequest);
        var result = await _eventRepository.GetFilteredAsync(
            page, pageSize, filterRequest.DateFrom, filterRequest.DateTo,
            filterRequest.Location, filterRequest.Categories,
            filterRequest.MaxParticipants, filterRequest.AvailableSpaces, cst);
        return _mapper.Map<PagedResponse<EventDto>>(result);
    }

    public async Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId, int pageNumber, int pageSize, CancellationToken cst = default)
    {
        var result = await _eventRepository.GetParticipantsAsync(eventId, pageNumber, pageSize, cst);
        return _mapper.Map<PagedResponse<ParticipantDto>>(result);
    }

    public async Task<Guid> RegisterAsync(Guid eventId, CancellationToken cst)
    {
        var userId = _accountService.GetCurrentUserId();
        var eventById = await _eventRepository.GetByIdAsync(eventId, cst)
            ?? throw new NotFoundException($"Event with id {eventId} not found");

        var user = await _userRepository.GetUserById(userId, cst)
            ?? throw new NotFoundException($"User with id: {userId} not found");

        var newParticipant = Participant.Create(
            userId, eventId, DateTime.UtcNow,
            user.FirstName, user.LastName, user.DateOfBirth);

        eventById.AddParticipant(newParticipant);
        await _eventRepository.UpdateAsync(eventById, cst);
        return userId;
    }

    public async Task CancelAsync(Guid eventId, CancellationToken cst = default)
    {
        var userId = _accountService.GetCurrentUserId();
        var eventById = await _eventRepository.GetByIdAsync(eventId, cst)
            ?? throw new NotFoundException($"Event with id {eventId} not found");

        var participant = eventById.Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw new NotFoundException("Participation not found");

        eventById.RemoveParticipant(userId);
        await _eventRepository.UpdateAsync(eventById, cst);
    }
}



