using AutoMapper;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Mapping;
using EventManager.Application.Requests;
using EventManager.Application.Services;
using EventManager.Domain.Models;
using EventManager.Persistence;
using EventManager.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.Tests.ServicesTests;


public class EventServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly EventRepository _eventRepository;

    // Моки для внешних зависимостей
    private readonly Mock<IFileStorage> _fileStorageMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    // Общие константы для тестовых данных
    private const string DefaultCategory = "Sports";
    private const string DefaultDescription = "Test Description";
    private const int DefaultMaxParticipants = 10;

    // Тестируемый сервис
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        // Arrange: Настройка in-memory базы данных
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Добавление тестовой категории (требуется для корректной работы методов репозитория)
        SeedTestCategory();

        // Настройка AutoMapper для основного слоя
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<EventProfile>());
        _mapper = mapperConfig.CreateMapper();

        // Настройка AutoMapper для слоя Persistence
        var mapperConfigForPersistance = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventManager.Persistence.Mapping.EventProfile>();
            cfg.AddProfile<EventManager.Persistence.Mapping.ParticipantProfile>();
        });
        IMapper mapperForPersistance = mapperConfigForPersistance.CreateMapper();

        // Создание  репозитория с in-memory контекстом
        _eventRepository = new EventRepository(_context, mapperForPersistance);

        // Создание моков для внешних сервисов
        _fileStorageMock = new Mock<IFileStorage>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Настройка методов транзакций UnitOfWork
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        _eventService = new EventService(
            _eventRepository,
            _mapper,
            _fileStorageMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    private void SeedTestCategory()
    {
        _context.Categories.Add(new EventManager.Persistence.Entities.CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = DefaultCategory
        });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region Test Data Helpers    

    // Общий метод для настройки мока пользователя
    private void SetupUserMock(Guid userId, User user) =>
        _userRepositoryMock.Setup(r => r.GetUserById(userId)).ReturnsAsync(user);

    /// <summary>
    /// Создает CreateEventRequest по умолчанию.
    /// Если передан делегат configure, он будет применен к запросу.
    /// </summary>
    private CreateEventRequest CreateDefaultEventRequest(
    Func<CreateEventRequest, CreateEventRequest> configure = null)
    {
        var request = new CreateEventRequest(
            Name: Guid.NewGuid().ToString(),
            Description: DefaultDescription,
            DateTime: DateTime.UtcNow.AddDays(1),
            Location: "Test Location",
            Category: DefaultCategory,
            MaxParticipants: DefaultMaxParticipants,
            ImageUrls: new List<string>()
        );

        return configure != null ? configure(request) : request;
    }

    /// <summary>
    /// Асинхронно создает тестовое событие, используя запрос по умолчанию.
    /// Если передан делегат configure, он будет применен к запросу.
    /// </summary>
    private async Task<Guid> CreateTestEventAsync(
    Func<CreateEventRequest, CreateEventRequest> configure = null)
    {
        // Создаем запрос по умолчанию и применяем настройки, если переданы
        var request = CreateDefaultEventRequest(configure);

        // Создаем событие и возвращаем его идентификатор
        return await _eventService.CreateAsync(request);
    }


    /// <summary>
    ///  Создание тестового пользователя и добавление его в контекст
    ///  чтобы прошла проверка в UpdateAsync в EventRepository репозитории, что такой пользователь существует
    /// </summary>
    private async Task<User> CreateAndAddUserAsync(Guid userId = default)
    {
        var user = User.Create(
            $"{Guid.NewGuid()}@test.com",
            "Test",
            "User",
            DateTime.UtcNow.AddYears(-20)
        );

        if (userId != default) user.Id = userId;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    #endregion

    #region OperationsWithEventTests
    [Fact(DisplayName = "GetAllAsync: Возвращает PagedResponce с событиями")]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        await CreateTestEventAsync();

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
        var request = CreateDefaultEventRequest(r => r with { Name = "Unique Name" });

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
        var request = CreateDefaultEventRequest();
        await _eventService.CreateAsync(request);

        // Act & Assert
        await _eventService.Invoking(s => s.CreateAsync(request))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "DeleteAsync: Работает удаление существующего события")]
    public async Task DeleteAsync_WithExistingEvent_DeletesEvent()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();

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
        var eventId = await CreateTestEventAsync();

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
        var eventId = await CreateTestEventAsync();
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

    [Fact(DisplayName = "UpdateAsync: Происходит exception при пустом описании в UpdateEventRequest")]
    public async Task UpdateAsync_WithEmptyDescription_ThrowsException()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        var updateRequest = new UpdateEventRequest(
            Description: "  ",
            DateTime: DateTime.UtcNow.AddDays(1),
            Location: "Location",
            MaxParticipants: 15
        );

        // Act & Assert
        await _eventService.Invoking(s => s.UpdateAsync(eventId, updateRequest))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact(DisplayName = "GetFilteredAsync: Корректно возвращает отфильтрованные результаты")]
    public async Task GetFilteredAsync_ReturnsFilteredResults()
    {
        // Arrange
        string location1 = "Location A";
        string location2 = "Location B";

        await CreateTestEventAsync(r => r with { Location = location1 });
        await CreateTestEventAsync(r => r with { Location = location2 });

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
        var eventId = await CreateTestEventAsync();

        var user = await CreateAndAddUserAsync();
        SetupUserMock(user.Id, user);

        var registerRequest = new RegisterParticipantRequest
        {
            EventId = eventId,
            UserId = user.Id,
            RegistrationDate = DateTime.UtcNow
        };

        // Act
        var resultUserId = await _eventService.RegisterAsync(registerRequest);

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
        var user = await CreateAndAddUserAsync();
        SetupUserMock(user.Id, user);

        var registerRequest = new RegisterParticipantRequest
        {
            EventId = Guid.NewGuid(),
            UserId = user.Id,
            RegistrationDate = DateTime.UtcNow
        };

        // Act & Assert
        await _eventService.Invoking(s => s.RegisterAsync(registerRequest))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "RegisterAsync: Происходит exception при добавлении в событие не существующего пользователя")]
    public async Task RegisterAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();

        // Не добавляем пользователя в контекст, чтобы он не был найден
        var missingUserId = Guid.NewGuid();
        SetupUserMock(missingUserId, null);

        var registerRequest = new RegisterParticipantRequest
        {
            EventId = eventId,
            UserId = missingUserId,
            RegistrationDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _eventService.RegisterAsync(registerRequest));
    }

    [Fact(DisplayName = "GetParticipantsAsync: Корректно возвращает список участников")]
    public async Task GetParticipantsAsync_ReturnsRegisteredParticipant()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        var user = await CreateAndAddUserAsync();
        SetupUserMock(user.Id, user);  

        await _eventService.RegisterAsync(new RegisterParticipantRequest
        {
            EventId = eventId,
            UserId = user.Id,
            RegistrationDate = DateTime.UtcNow
        });

        // Act
        var result = await _eventService.GetParticipantsAsync(eventId, 1, 10);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data.First().UserId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "CancelAsync: Корректно даляет участника из события")]
    public async Task CancelAsync_WithValidData_RemovesParticipant()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();
        var user = await CreateAndAddUserAsync();
        SetupUserMock(user.Id, user);

        await _eventService.RegisterAsync(new RegisterParticipantRequest
        {
            EventId = eventId,
            UserId = user.Id,
            RegistrationDate = DateTime.UtcNow
        });

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
        var user = await CreateAndAddUserAsync();
        SetupUserMock(user.Id, user);

        // Act & Assert
        await _eventService.Invoking(s => s.CancelAsync(Guid.NewGuid(), user.Id))
            .Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "CancelAsync: Происходит exception, если пользователь не найден")]
    public async Task CancelAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var eventId = await CreateTestEventAsync();

        // Добавляем корректного пользователя для успешного создания события,
        // но для отмены регистрации будем запрашивать другого пользователя, которого нет в базе.
        var missingUserId = Guid.NewGuid();
        SetupUserMock(missingUserId, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _eventService.CancelAsync(eventId, missingUserId));
    }
    #endregion
}
