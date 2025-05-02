using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Mapping.EventProfiles;
using EventManager.Application.Mapping.PagedResponceProfiles;
using EventManager.Application.Mapping.ParticipantProfiles;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using EventManager.Persistence;
using EventManager.Persistence.Mapping.EventProfiles;
using EventManager.Persistence.Mapping.ParticipantProfiles;
using Microsoft.EntityFrameworkCore;
using Moq;
using EventManager.Persistence.Mapping.CategoryProfiles;

namespace Application.Tests.Factories;

public static class EventTestFactory
{
    // Константы для значений по умолчанию
    //public const string DefaultCategory = "Sports";
    public static readonly Guid defaultCategoryId = Guid.NewGuid();
    public const string DefaultDescription = "Test Description";
    public const int DefaultMaxParticipants = 10;
    public const string DefaultLocation = "Test Location";

    #region Event Helpers

    /// <summary>
    /// Создает CreateEventRequest по умолчанию.
    /// Если передан делегат configure, он будет применен к запросу.
    /// </summary>
    public static CreateEventRequest CreateDefaultEventRequest(
        Func<CreateEventRequest, CreateEventRequest> configure = null)
    {
        var request = new CreateEventRequest(
            Name: Guid.NewGuid().ToString(),
            Description: DefaultDescription,
            DateTime: DateTime.UtcNow.AddDays(1),
            Location: DefaultLocation,
            CategoryId: defaultCategoryId,
            MaxParticipants: DefaultMaxParticipants);

        return configure != null ? configure(request) : request;
    }

    /// <summary>
    /// Асинхронно создает тестовое событие, используя запрос по умолчанию.
    /// Если передан делегат configure, он будет применен к запросу.
    /// </summary>
    public static async Task<Guid> CreateTestEventAsync(
        IEventService eventService,
        Func<CreateEventRequest, CreateEventRequest> configure = null)
    {
        // Создаем запрос по умолчанию и применяем настройки, если переданы
        var request = CreateDefaultEventRequest(configure);

        // Создаем событие и возвращаем его идентификатор
        return await eventService.CreateAsync(request);
    }

    #endregion

    #region User Helpers

    public static void SetupUserRepositoryMock(
        Mock<IUserRepository> userRepositoryMock,
        Guid userId,
        User user)
    {
        userRepositoryMock
             .Setup(r => r.GetUserById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }


    public static void SetupAccountServiceMock(
        Mock<IAccountService> accountServiceMock,
        Guid userId)
    {
        accountServiceMock
          .Setup(s => s.GetCurrentUserId())
          .Returns(userId);
    }

    /// <summary>
    ///  Создание тестового пользователя и добавление его в контекст
    ///  чтобы прошла проверка в UpdateAsync в EventRepository репозитории, что такой пользователь существует
    /// </summary>
    public static async Task<User> CreateAndAddUserAsync(
        ApplicationDbContext context,
        Guid? userId = null)
    {
        var user = User.Create(
            $"{Guid.NewGuid()}@test.com",
            "Test",
            "User",
            DateTime.UtcNow.AddYears(-20));

        if (userId.HasValue)
            user.Id = userId.Value;

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    #endregion

    #region Context Configuration

    public static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        SeedTestCategory(context);
        return context;
    }

    private static void SeedTestCategory(ApplicationDbContext context)
    {
        context.Categories.Add(new EventManager.Persistence.Entities.CategoryEntity
        {
            Id = defaultCategoryId,
            Name = "TestCategoryName"
        });
        context.SaveChanges();
    }

    #endregion

    #region Mapper Configuration

    public static Mock<IAccountService> CreateAccountServiceMock(Guid? userId = null)
    {
        var mock = new Mock<IAccountService>();
        mock.Setup(a => a.GetCurrentUserId())
            .Returns(userId ?? Guid.NewGuid());
        return mock;
    }

    public static IMapper CreateApplicationMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(EventToEventDtoProfile).Assembly);
        });
        return config.CreateMapper();
    }

    public static IMapper CreatePersistenceMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            // Сканируем сборку Persistence для профилей
            cfg.AddMaps(typeof(EventEntityToEventProfile).Assembly);
        });
        return config.CreateMapper();
    }

    #endregion
}