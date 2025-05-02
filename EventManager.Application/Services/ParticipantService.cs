using AutoMapper;
using EventManager.Application.Dtos;
using EventManager.Application.Exceptions;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Models;

namespace EventManager.Application.Services;

public class ParticipantService : IParticipantService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public ParticipantService(
        IParticipantRepository participantRepository,
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IAccountService accountService,
        IMapper mapper)
    {
        _participantRepository = participantRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _accountService = accountService;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(Guid eventId, int pageNumber, int pageSize, CancellationToken cst = default)
    {
        var eventById = await _eventRepository.GetByIdAsync(eventId, cst)
          ?? throw new NotFoundException($"Event with id {eventId} not found");
        var result = await _participantRepository.GetParticipantsAsync(eventId, pageNumber, pageSize, cst);
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
        await _participantRepository.AddAsync(newParticipant, cst);
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
        await _participantRepository.RemoveAsync(participant, cst);
    }
}