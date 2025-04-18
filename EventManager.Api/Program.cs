using EventManager.Api.Extensions;
using EventManager.API.Handlers;
using EventManager.Domain.Interfaces;
using EventManager.Application.Interfaces.AuthTokenProcessor;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Services;
using EventManager.Application.Validators;
using EventManager.Domain.Options;
using EventManager.Infrastructure.Processors;
using EventManager.Persistence;
using EventManager.Persistence.Repositories;
using EventManager.Persistence.UnitOfWork;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using EventManager.Infrastructure.Services;
using EventManager.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(
builder.Configuration.GetSection(JwtOptions.JwtOptionsKey));

// ���������� Identity ��� ������������ � ����������� ������������ ������
// � ������������ ����������� �����,
// � ����������� � identity information stores ����� Entity Framework.
builder.Services.AddIdentityWithPasswordAndEmailSecurity();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"));
});

//������������ redis � CacheOptions
builder.Services.Configure<CacheOptions>(
    builder.Configuration.GetSection("CacheOptions"));
builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration.GetConnectionString("Cache"));

builder.Services.AddAutoMapper(
    typeof(EventManager.Persistence.Mapping.EventProfile).Assembly
);

//����������� � DI ���������
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();

//����������� ������������
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();




//����������� ��������
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateEventRequestValidator>();

builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//��������� � ��������� DI ������ IHttpContextAccessor, ��� ������ � HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery();

var app = builder.Build();



var uploadPath = Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads";
if (!Directory.Exists(uploadPath))
    Directory.CreateDirectory(uploadPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "EventManager");
    });
    app.ApplyMigrations();
}


app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.AddMappedEndpoints();

app.Run();


