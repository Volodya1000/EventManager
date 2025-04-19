using AutoMapper;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using EventManager.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.Tests.Factories;

public static class EventTestFactory
{
    // Константы для значений по умолчанию
    public const string DefaultCategory = "Sports";
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
            Category: DefaultCategory,
            MaxParticipants: DefaultMaxParticipants,
            ImageUrls: new List<string>());

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
            .Setup(r => r.GetUserById(userId))
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

    //public static void SetupUserServiceMock(
    //Mock<IAccountService> userServiceMock,
    //Guid userId,
    //UserDto userDto) // Используем DTO вместо доменной модели
    //{
    //    userServiceMock
    //        .Setup(s => s.GetUserByIdAsync(userId))
    //        .ReturnsAsync(userDto); // Предполагаем асинхронный метод

    //    Если нужно обрабатывать несуществующих пользователей
    //    userServiceMock
    //        .Setup(s => s.GetUserByIdAsync(It.Is<Guid>(id => id != userId))
    //        .ReturnsAsync((UserDto?)null);
    //}


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
        if (!context.Categories.Any(c => c.Name == DefaultCategory))
        {
            context.Categories.Add(new EventManager.Persistence.Entities.CategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = DefaultCategory
            });
            context.SaveChanges();
        }
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
            cfg.AddProfile<EventManager.Application.Mapping.EventProfile>());
        return config.CreateMapper();
    }

    public static IMapper CreatePersistenceMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<EventManager.Persistence.Mapping.EventProfile>();
            cfg.AddProfile<EventManager.Persistence.Mapping.ParticipantProfile>();
        });
        return config.CreateMapper();
    }

    #endregion
}