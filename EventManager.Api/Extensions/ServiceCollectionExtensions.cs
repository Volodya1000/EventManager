using EventManager.Application.Interfaces.AuthTokenProcessor;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Mapping.EventProfiles;
using EventManager.Application.Services;
using EventManager.Application.Validators;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Interfaces;
using EventManager.Infrastructure.Options;
using EventManager.Infrastructure.Services;
using EventManager.Persistence.Mapping.EventProfiles;
using EventManager.Persistence.Repositories;
using EventManager.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DbConnectionString"));
        });
    }

    public static void AddCacheConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Cache"));
    }

    public static void AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddAutoMapper(
            typeof(EventEntityToEventProfile).Assembly,
            typeof(EventToEventDtoProfile).Assembly
        );
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IParticipantRepository, ParticipantRepository>();
    }

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IParticipantService, ParticipantService>();
    }

    public static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateEventRequestValidator>();
    }
}