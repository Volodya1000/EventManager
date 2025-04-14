using AutoMapper;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Mapping;
using EventManager.Application.Requests;
using EventManager.Application.Services;
using EventManager.Persistence;
using EventManager.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        // Arrange: Создание реального репозитория с in-memory контекстом
        _eventRepository = new EventRepository(_context, mapperForPersistance);

        // Arrange: Создание моков для внешних сервисов
        _fileStorageMock = new Mock<IFileStorage>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // Настройка методов транзакций UnitOfWork
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackAsync()).Returns(Task.CompletedTask);

        // Создание экземпляра тестируемого сервиса
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

}
