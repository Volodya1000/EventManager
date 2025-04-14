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

    // Тестируемый сервис
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        // Arrange: Настройка in-memory базы данных
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // Добавление тестовой категории (требуется для методов репозитория)
        _context.Categories.Add(new EventManager.Persistence.Entities.CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Sports"
        });
        _context.SaveChanges();

        // Настройка AutoMapper для основного слоя
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Настройка AutoMapper для слоя Persistence
        var mapperConfigForPersistance = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventManager.Persistence.Mapping.EventProfile>();
            cfg.AddProfile<EventManager.Persistence.Mapping.ParticipantProfile>();
        });
        IMapper mapperForPersistance = mapperConfigForPersistance.CreateMapper();

        // Создание реального репозитория с in-memory контекстом
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


    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region OperationsWithEventTests


    [Fact]
    public async Task GetAllAsync_ReturnsPagedResponse()
    {
        // Arrange
        var createRequest = new CreateEventRequest(
            Name: "Test Event",
            Description: "Description",
            DateTime: DateTime.UtcNow.AddDays(1),
            Location: "Test Location",
            Category: "Sports",
            MaxParticipants: 10,
            ImageUrls: new List<string> ()
        );
        await _eventService.CreateAsync(createRequest);

        // Act
        var result = await _eventService.GetAllAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesEvent()
    {
        // Arrange
        var request = new CreateEventRequest(
            Name: "Unique Event",
            Description: "Valid Description",
            DateTime: DateTime.UtcNow.AddDays(2),
            Location: "Location A",
            Category: "Sports",
            MaxParticipants: 20,
            ImageUrls: new List<string>()
        );

        // Act
        var eventId = await _eventService.CreateAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, eventId);
        var createdEvent = await _eventRepository.GetByIdAsync(eventId);
        Assert.NotNull(createdEvent);
        Assert.Equal(request.Name, createdEvent.Name);
    }


    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsException()
    {
        // Arrange
        var request = new CreateEventRequest(
            Name: "Duplicate Event",
            Description: "Desc",
            DateTime: DateTime.UtcNow.AddDays(3),
            Location: "Location B",
            Category: "Sports",
            MaxParticipants: 15,
            ImageUrls: new List<string>()
        );
        
        await _eventService.CreateAsync(request);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _eventService.CreateAsync(request));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEvent_DeletesEvent()
    {
        // Arrange
        var request = new CreateEventRequest(
            Name: "Event To Delete",
            Description: "Desc",
            DateTime: DateTime.UtcNow.AddDays(5),
            Location: "Delete Location",
            Category: "Sports",
            MaxParticipants: 10,
            ImageUrls: new List<string>()
        );
        var eventId = await _eventService.CreateAsync(request);

        // Act
        await _eventService.DeleteAsync(eventId);

        // Assert
        var deletedEvent = await _eventRepository.GetByIdAsync(eventId);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEventDto()
    {
        // Arrange
        var request = new CreateEventRequest(
            Name: "Event GetById",
            Description: "Desc",
            DateTime: DateTime.UtcNow.AddDays(4),
            Location: "Location",
            Category: "Sports",
            MaxParticipants: 12,
            ImageUrls: new List<string>()
        );
        var eventId = await _eventService.CreateAsync(request);

        // Act
        var result = await _eventService.GetByIdAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesEvent()
    {
        // Arrange
        var request = new CreateEventRequest(
            Name: "Event Update",
            Description: "Old Description",
            DateTime: DateTime.UtcNow.AddDays(4),
            Location: "Old Location",
            Category: "Sports",
            MaxParticipants: 20,
            ImageUrls: new List<string>()
        );
        var eventId = await _eventService.CreateAsync(request);

        var updateRequest = new UpdateEventRequest(
            Description: "New Description",
            DateTime: DateTime.UtcNow.AddDays(4), 
            Location: "New Location",
            MaxParticipants: 25
        );

        // Act
        await _eventService.UpdateAsync(eventId, updateRequest);

        // Assert
        var updatedEvent = await _eventRepository.GetByIdAsync(eventId);
        Assert.Equal(updateRequest.Description, updatedEvent.Description);
        Assert.Equal(updateRequest.Location, updatedEvent.Location);
        Assert.Equal(updateRequest.MaxParticipants, updatedEvent.MaxParticipants);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyDescription_ThrowsException()
    {
        // Arrange
        var request = new CreateEventRequest(
            Name: "Event EmptyDesc",
            Description: "Valid Desc",
            DateTime: DateTime.UtcNow.AddDays(4),
            Location: "Location",
            Category: "Sports",
            MaxParticipants: 15,
            ImageUrls: new List<string>()
        );
        var eventId = await _eventService.CreateAsync(request);

        var updateRequest = new UpdateEventRequest(
            Description: "  ", //Некорректное описание
            DateTime: DateTime.UtcNow.AddDays(4),
            Location: "NewLocation",
            MaxParticipants: 15
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _eventService.UpdateAsync(eventId, updateRequest));
    }

    [Fact]
    public async Task GetFilteredAsync_ReturnsFilteredResults()
    {
        // Arrange
        // Создание двух событий с разной локацией
        var request1 = new CreateEventRequest(
            Name: "Filtered Event 1",
            Description: "Desc",
            DateTime: DateTime.UtcNow.AddDays(5),
            Location: "New York",
            Category: "Sports",
            MaxParticipants: 20,
            ImageUrls: new List<string>()
        );
        var request2 = new CreateEventRequest(
            Name: "Filtered Event 2",
            Description: "Desc",
            DateTime: DateTime.UtcNow.AddDays(6),
            Location: "Los Angeles",
            Category: "Sports",
            MaxParticipants: 20,
            ImageUrls: new List<string>()
        );
        await _eventService.CreateAsync(request1);
        await _eventService.CreateAsync(request2);

        var filterRequest = new EventFilterRequest
        {
            Location = "New York"
        };

        // Act
        var result = await _eventService.GetFilteredAsync(filterRequest, page: 1, pageSize: 10);

        // Assert
        Assert.Single(result.Data);
        Assert.Equal("Filtered Event 1", result.Data.First().Name);
    }

    #endregion


    #region OperationsWithParticipants Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_RegistersParticipant()
    {
        // Arrange
        // Создаём событие
        var createEventRequest = new CreateEventRequest(
            Name: "Football Match",
            Description: "Exciting match",
            DateTime: DateTime.UtcNow.AddDays(2),
            Location: "Stadium",
            Category: "Sports",
            MaxParticipants: 20,
            ImageUrls: new List<string>()
        );
        var eventId = await _eventService.CreateAsync(createEventRequest);

        // Подготавливаем тестового пользователя и добавляем его в контекст,
        // чтобы UpdateAsync в репозитории проверил, что такой пользователь существует.
        var userId = Guid.NewGuid();
        var testUser = User.Create("user@example.com", "Alice", "Johnson", DateTime.UtcNow.AddYears(-25));
        testUser.Id = userId; // Принудительно задаём идентификатор

        // Важно добавить пользователя в базу, так как в UpdateAsync происходит проверка существования пользователя
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();

        // Настраиваем поведение userRepository, чтобы вернуть тестового пользователя
        _userRepositoryMock.Setup(r => r.GetUserById(userId))
            .ReturnsAsync(testUser);

        var registerRequest = new RegisterParticipantRequest
        {
            EventId = eventId,
            UserId = userId,
            RegistrationDate = DateTime.UtcNow
        };

        // Act
        var resultUserId = await _eventService.RegisterAsync(registerRequest);

        // Assert
        Assert.Equal(userId, resultUserId);
        var updatedEvent = await _eventRepository.GetByIdAsync(eventId);
        Assert.Contains(updatedEvent.Participants, p => p.UserId == userId);
    }

    
    #endregion

}
