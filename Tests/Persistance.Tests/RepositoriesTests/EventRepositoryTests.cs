using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence;
using EventManager.Persistence.Entities;
using EventManager.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Persistance.Tests.RepositoriesTests;

public class EventRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly EventRepository _repository;

    public EventRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg =>
            cfg.AddProfile<EventManager.Persistence.Mapping.EventProfile>());

        _mapper = mapperConfig.CreateMapper();
        _repository = new EventRepository(_context, _mapper);

        AddTestCategory();
    }

    private void AddTestCategory()
    {
        _context.Categories.Add(new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Sports"
        });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact(DisplayName = "AddAsync: Создает событие при валидных данных")]
    public async Task AddAsync_WithValidData_CreatesEvent()
    {
        // Arrange
        var newEvent = Event.Create(
            id: Guid.NewGuid(),
            name: "New Event",
            description: "Description of event",
            dateTime: DateTime.UtcNow.AddDays(1),
            location: "Location of event",
            category: "Sports",
            maxParticipants: 10,
            imageUrls: new List<string> { "https://example.com/url1", 
                                            "https://example.com/url2" }
             );

        // Act
        await _repository.AddAsync(newEvent);

        // Assert через методы репозитория
        var createdEvent = await _repository.GetByIdAsync(newEvent.Id);

        createdEvent.Should().NotBeNull();
        createdEvent.Name.Should().Be(newEvent.Name);
        createdEvent.Description.Should().Be(newEvent.Description);
        createdEvent.DateTime.Should().Be(newEvent.DateTime);
        createdEvent.Location.Should().Be(newEvent.Location);
        createdEvent.Category.Should().Be(newEvent.Category);
        createdEvent.MaxParticipants.Should().Be(newEvent.MaxParticipants);
        createdEvent.ImageUrls.Should().BeEquivalentTo(newEvent.ImageUrls);
    }

    private async Task SeedMultipleEvents(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            var @event = Event.Create(
                id: Guid.NewGuid(),
                name: $"Event {i}",
                description: $"Description of event {i}",
                dateTime: DateTime.UtcNow.AddDays(i),
                location: $"Location of event {i}",
                category: "Sports",
                maxParticipants: 10);

            await _repository.AddAsync(@event);
        }
    }

    [Fact(DisplayName = "GetAllAsync: Возвращает корректные данные пагинации")]
    public async Task GetAllAsync_ReturnsCorrectPagination()
    {
        // Arrange
        const int totalEvents = 15;
        await SeedMultipleEvents(totalEvents);

        // Act
        var page1 = await _repository.GetAllAsync(1, 5);
        var page2 = await _repository.GetAllAsync(2, 5);

        // Assert 
        page1.Data.Should().HaveCount(5);
        page2.Data.Should().HaveCount(5);
        page1.TotalRecords.Should().Be(totalEvents);
        page2.PageNumber.Should().Be(2);

        var allEvents = (await _repository.GetAllAsync(1, totalEvents)).Data;
        allEvents.Should().BeInAscendingOrder(e => e.DateTime);
    }
}