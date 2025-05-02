using Application.Tests.Factories;
using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Application.Services;
using EventManager.Application.Validators;
using EventManager.Persistence;
using FluentAssertions;
using Moq;

namespace Application.Tests.ServicesTests;

public class EventServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EventRepository _eventRepository;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _context = EventTestFactory.CreateInMemoryContext(); 

        var applictionMapper = EventTestFactory.CreateApplicationMapper();
        var persistenceMapper = EventTestFactory.CreatePersistenceMapper();

        _eventRepository = new EventRepository(_context, persistenceMapper);

        var categoryRepository = new CategoryRepository(_context, persistenceMapper);
        var userRepositoryMock = new Mock<IUserRepository>();
        var accountServiceMock = new Mock<IAccountService>();

        _eventService = new EventService(
            _eventRepository,
            categoryRepository,
            applictionMapper,
            userRepositoryMock.Object,
            accountServiceMock.Object,
            new CreateEventRequestValidator(),
            new UpdateEventRequestValidator(),
            new EventFilterRequestValidator());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

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


    [Fact(DisplayName = "CreateAsync: Происходит exception при повторяющемся имени события")]
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
}
