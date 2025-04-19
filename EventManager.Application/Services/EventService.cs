using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using EventManager.Application.Exceptions;
using FluentValidation;

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


    #region OperationsWithEvents
    public async Task<PagedResponse<EventDto>> GetAllAsync(int page, int pageSize)
    {
        return await _eventRepository.GetAllAsync(page, pageSize);
    }

    public async Task<Guid> CreateAsync(CreateEventRequest request)
    {
        _createValidator.ValidateAndThrow(request);

        // Проверка уникальности имени события
        var exists = await _eventRepository.GetByNameAsync(request.Name);
        if (exists != null)
            throw new InvalidOperationException("Event with this name already exists");

        var categoryExists = await _categoryRepository.ExistsAsync(request.Category);
        if (!categoryExists)
            throw new NotFoundException($"Category {request.Category} not found");

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
        _updateValidator.ValidateAndThrow(request);

        var eventById = await _eventRepository.GetByIdAsync(eventId)
           ?? throw new NotFoundException($"Event with id {eventId} not found");


        eventById.UpdateDescription(request.Description);

        eventById.UpdateLocation(request.Location);

        eventById.UpdateMaxParticipants(request.MaxParticipants);


        await _eventRepository.UpdateAsync(eventById);
    }

    public async Task<PagedResponse<EventDto>> GetEventsByUserAsync(int page, int pageSize)
    {
        var userId = _accountService.GetCurrentUserId();

        return await _eventRepository.GetEventsByUserAsync(userId, page, pageSize);
    }
    #endregion

    #region OperationsWithParticipants
    public async Task<PagedResponse<EventDto>> GetFilteredAsync(EventFilterRequest filterRequest, int page, int pageSize)
    {
        _filterValidator.ValidateAndThrow(filterRequest);

        return await _eventRepository.GetFilteredAsync(filterRequest, page, pageSize);
    }

    public async Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId,int pageNumber,int pageSize)
    {
        return await _eventRepository.GetParticipantsAsync(eventId, pageNumber, pageSize);
    }

    public async Task<Guid> RegisterAsync(Guid eventId)
    {
        var userId = _accountService.GetCurrentUserId();

        var eventById = await _eventRepository.GetByIdAsync(eventId)
            ??  throw new NotFoundException($"Event with id {eventId} not found");

        var user = await _userRepository.GetUserById(userId);

        if (user == null) 
            throw new NotFoundException($"User with id: {userId} not found");

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

    public async Task CancelAsync(Guid eventId)
    {
        var userId = _accountService.GetCurrentUserId();

        var eventById = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new NotFoundException($"Event with id {eventId} not found");

        var participant = eventById.Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw new NotFoundException("Participation not found");

        eventById.RemoveParticipant(userId);

        await _eventRepository.UpdateAsync(eventById);
    }


    #endregion
}



