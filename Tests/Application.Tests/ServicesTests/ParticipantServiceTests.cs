using Application.Tests.Factories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Services;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Persistence.Repositories;
using EventManager.Persistence;
using Moq;
using EventManager.Application.Exceptions;
using AutoMapper;
using EventManager.Application.Validators;
using FluentAssertions;

namespace Application.Tests.ServicesTests;

public class ParticipantServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IParticipantRepository _participantRepository;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly ParticipantService _participantService;
    private readonly EventService _eventService;

    public ParticipantServiceTests()
    {
        _context = EventTestFactory.CreateInMemoryContext();

        var persistenceMapper = EventTestFactory.CreatePersistenceMapper();
        var applicationMapper = EventTestFactory.CreateApplicationMapper();

        var eventRepository = new EventRepository(_context, persistenceMapper);
        _participantRepository = new ParticipantRepository(_context, persistenceMapper);

        var categoryRepository = new CategoryRepository(_context, persistenceMapper);
        _userRepositoryMock = new Mock<IUserRepository>();
        _accountServiceMock = new Mock<IAccountService>();

        _eventService = new EventService(
            eventRepository,
            categoryRepository,
            applicationMapper,
            _userRepositoryMock.Object,
            _accountServiceMock.Object,
            new CreateEventRequestValidator(),
            new UpdateEventRequestValidator(),
            new EventFilterRequestValidator());

        _participantService = new ParticipantService(
            _participantRepository,
            eventRepository,
            _userRepositoryMock.Object,
            _accountServiceMock.Object,
            applicationMapper);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact(DisplayName = "RegisterAsync: Регистрирует участника с валидными данными")]
    public async Task RegisterAsync_WithValidData_RegistersParticipant()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);

        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, user.Id, user);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, user.Id);

        // Act
        var resultUserId = await _participantService.RegisterAsync(eventId, CancellationToken.None);

        // Assert
        resultUserId.Should().Be(user.Id);

        var participants = await _participantRepository.GetParticipantsAsync(eventId, 1, 10);
        participants.Data.Should().ContainSingle(p => p.UserId == user.Id);
    }

    [Fact(DisplayName = "RegisterAsync: Происходит exception при отсутствии события")]
    public async Task RegisterAsync_WhenEventNotFound_ThrowsException()
    {
        // Arrange
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, user.Id, user);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, user.Id);

        // Act & Assert
        await _participantService.Invoking(s =>
            s.RegisterAsync(Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "RegisterAsync: Происходит exception при добавлении несуществующего пользователя")]
    public async Task RegisterAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var missingUserId = Guid.NewGuid();

        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, missingUserId, null);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, missingUserId);

        // Act & Assert
        await _participantService.Invoking(s =>
            s.RegisterAsync(eventId, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "GetParticipantsAsync: Корректно возвращает список участников")]
    public async Task GetParticipantsAsync_ReturnsRegisteredParticipant()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);

        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, user.Id, user);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, user.Id);

        await _participantService.RegisterAsync(eventId, CancellationToken.None);

        // Act
        var result = await _participantService.GetParticipantsAsync(eventId, 1, 10);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data.First().UserId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "CancelAsync: Корректно удаляет участника из события")]
    public async Task CancelAsync_WithValidData_RemovesParticipant()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);

        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, user.Id, user);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, user.Id);

        await _participantService.RegisterAsync(eventId, CancellationToken.None);

        // Act
        await _participantService.CancelAsync(eventId);

        // Assert
        var participants = await _participantRepository.GetParticipantsAsync(eventId, 1, 10);
        participants.Data.Should().BeEmpty();
    }

    [Fact(DisplayName = "CancelAsync: Ошибка отмены участия - событие не найдено")]
    public async Task CancelAsync_WhenEventNotFound_ThrowsException()
    {
        // Arrange
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, user.Id, user);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, user.Id);

        // Act & Assert
        await _participantService.Invoking(s =>
            s.CancelAsync(Guid.NewGuid()))
            .Should().ThrowExactlyAsync<NotFoundException>();
    }

    [Fact(DisplayName = "CancelAsync: Происходит exception, если пользователь не найден")]
    public async Task CancelAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var missingUserId = Guid.NewGuid();

        EventTestFactory.SetupUserRepositoryMock(_userRepositoryMock, missingUserId, null);
        EventTestFactory.SetupAccountServiceMock(_accountServiceMock, missingUserId);

        // Act & Assert
        await _participantService.Invoking(s =>
            s.CancelAsync(eventId))
            .Should().ThrowAsync<NotFoundException>();
    }
}