using Application.Tests.Factories;
using AutoMapper;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Requests;
using EventManager.Application.Services;
using EventManager.Application.Validators;
using EventManager.Persistence;
using EventManager.Persistence.Repositories;
using FluentAssertions;
using Moq;

namespace Application.Tests.ServicesTests;


public class EventServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly EventRepository _eventRepository;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _context = EventTestFactory.CreateInMemoryContext(); 

        _mapper = EventTestFactory.CreateApplicationMapper();

        var persistenceMapper = EventTestFactory.CreatePersistenceMapper();
        _eventRepository = new EventRepository(_context, persistenceMapper);

        _userRepositoryMock = new Mock<IUserRepository>();

        _eventService = new EventService(
            _eventRepository,
            _mapper,
            _userRepositoryMock.Object,
            new CreateEventRequestValidator(),
            new UpdateEventRequestValidator(),
            new EventFilterRequestValidator());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region OperationsWithEventTests
    [Fact(DisplayName = "GetAllAsync: Возвращает PagedResponce с событиями")]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        await EventTestFactory.CreateTestEventAsync(_eventService);

        // Act
        var result = await _eventService.GetAllAsync(page: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.TotalRecords.Should().Be(1);
    }

    [Fact(DisplayName = "CreateAsync: Создает событие с валидными данными")]
    public async Task CreateAsync_WithValidData_CreatesEvent()
    {
        // Arrange
        var request = EventTestFactory.CreateDefaultEventRequest(r => r with { Name = "Unique Name" });

        // Act
        var eventId = await _eventService.CreateAsync(request);

        // Assert
        eventId.Should().NotBe(Guid.Empty);

        var createdEvent = await _eventRepository.GetByIdAsync(eventId);
        createdEvent.Should().NotBeNull();
        createdEvent.Name.Should().Be(request.Name);
    }


    [Fact(DisplayName = "CreateAsync: Ппроисходит exception при повторяющемся имени события")]
    public async Task CreateAsync_WithDuplicateName_ThrowsException()
    {
        // Arrange
        var request = EventTestFactory.CreateDefaultEventRequest();
        await _eventService.CreateAsync(request);

        // Act & Assert
        await _eventService.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "DeleteAsync: Работает удаление существующего события")]
    public async Task DeleteAsync_WithExistingEvent_DeletesEvent()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);

        // Act
        await _eventService.DeleteAsync(eventId);

        // Assert
        var deletedEvent = await _eventRepository.GetByIdAsync(eventId);
        deletedEvent.Should().BeNull();
    }

    [Fact(DisplayName = "GetByIdAsync: Возвращает события по ID")]
    public async Task GetByIdAsync_WithValidId_ReturnsEventDto()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);

        // Act
        var result = await _eventService.GetByIdAsync(eventId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(eventId);
    }

    [Fact(DisplayName = "UpdateAsync: Обновление данных события")]
    public async Task UpdateAsync_WithValidData_UpdatesEvent()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var updateRequest = new UpdateEventRequest(
            Description: "Updated Description",
            DateTime: DateTime.UtcNow.AddDays(2),
            Location: "New Location",
            MaxParticipants: 20
        );

        // Act
        await _eventService.UpdateAsync(eventId, updateRequest);

        // Assert
        var updatedEvent = await _eventRepository.GetByIdAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent.Description.Should().Be(updateRequest.Description);
        updatedEvent.Location.Should().Be(updateRequest.Location);
        updatedEvent.MaxParticipants.Should().Be(updateRequest.MaxParticipants);
    }

    [Fact(DisplayName = "UpdateAsync: Происходит ValidationException exception при пустом описании в UpdateEventRequest")]
    public async Task UpdateAsync_WithEmptyDescription_ThrowsException()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var updateRequest = new UpdateEventRequest(
            Description: "  ",
            DateTime: DateTime.UtcNow.AddDays(1),
            Location: "Location",
            MaxParticipants: 15
        );

        // Act & Assert
        await _eventService.Invoking(s => s.UpdateAsync(eventId, updateRequest))
            .Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "GetFilteredAsync: Корректно возвращает отфильтрованные результаты")]
    public async Task GetFilteredAsync_ReturnsFilteredResults()
    {
        // Arrange
        string location1 = "Location A";
        string location2 = "Location B";

        await EventTestFactory.CreateTestEventAsync(_eventService, r => r with { Location = location1 });
        await EventTestFactory.CreateTestEventAsync(_eventService, r => r with { Location = location2 });

        var filterRequest = new EventFilterRequest { Location = location1 };

        // Act
        var result = await _eventService.GetFilteredAsync(filterRequest, page: 1, pageSize: 10);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data.First().Location.Should().Be(location1);
    }

    #endregion


    #region OperationsWithParticipants Tests

    [Fact(DisplayName = "RegisterAsync: Регистрирует участника с валидными данными")]
    public async Task RegisterAsync_WithValidData_RegistersParticipant()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserMock(_userRepositoryMock, user.Id, user);

        // Act
        var resultUserId = await _eventService.RegisterAsync(eventId, user.Id);

        // Assert
        resultUserId.Should().Be(user.Id);
        var updatedEvent = await _eventRepository.GetByIdAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent.Participants.Should().Contain(p => p.UserId == user.Id);
    }

    [Fact(DisplayName = "RegisterAsync: Происходит exception при отсутствии события")]
    public async Task RegisterAsync_WhenEventNotFound_ThrowsException()
    {
        // Arrange
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserMock(_userRepositoryMock, user.Id, user);

        // Act & Assert
        await _eventService.Invoking(s => s.RegisterAsync(Guid.NewGuid(), user.Id))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "RegisterAsync: Происходит exception при добавлении в событие не существующего пользователя")]
    public async Task RegisterAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var missingUserId = Guid.NewGuid();
        EventTestFactory.SetupUserMock(_userRepositoryMock, missingUserId, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _eventService.RegisterAsync(eventId, missingUserId));
    }

    [Fact(DisplayName = "GetParticipantsAsync: Корректно возвращает список участников")]
    public async Task GetParticipantsAsync_ReturnsRegisteredParticipant()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserMock(_userRepositoryMock, user.Id, user);
        await _eventService.RegisterAsync(eventId, user.Id);

        // Act
        var result = await _eventService.GetParticipantsAsync(eventId, 1, 10);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data.First().UserId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "CancelAsync: Корректно eдаляет участника из события")]
    public async Task CancelAsync_WithValidData_RemovesParticipant()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserMock(_userRepositoryMock, user.Id, user);
        await _eventService.RegisterAsync(eventId, user.Id);

        // Act
        await _eventService.CancelAsync(eventId, user.Id);

        // Assert
        var updatedEvent = await _eventRepository.GetByIdAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent.Participants.Should().BeEmpty();
    }

    [Fact(DisplayName = "CancelAsync: Ошибка отмены участия - событие не найдено")]
    public async Task CancelAsync_WhenEventNotFound_ThrowsException()
    {
        // Arrange
        var user = await EventTestFactory.CreateAndAddUserAsync(_context);
        EventTestFactory.SetupUserMock(_userRepositoryMock, user.Id, user); 

        // Act & Assert
        await _eventService.Invoking(s => s.CancelAsync(Guid.NewGuid(), user.Id))
            .Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "CancelAsync: Происходит exception, если пользователь не найден")]
    public async Task CancelAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var eventId = await EventTestFactory.CreateTestEventAsync(_eventService);
        // Добавляем корректного пользователя для успешного создания события,
        // но для отмены регистрации будем запрашивать другого пользователя, которого нет в базе.

        var missingUserId = Guid.NewGuid();
        EventTestFactory.SetupUserMock(_userRepositoryMock, missingUserId, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _eventService.CancelAsync(eventId, missingUserId));
    }
    #endregion
}
