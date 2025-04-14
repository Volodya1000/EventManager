using EventManager.Api.Extensions;
using EventManager.API.Handlers;
using EventManager.Application.FileStorage;
using EventManager.Application.Interfaces;
using EventManager.Application.Interfaces.AuthTokenProcessor;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Services;
using EventManager.Application.Validators;
using EventManager.Domain.Constants;
using EventManager.Domain.Models;
using EventManager.Domain.Options;
using EventManager.Infrastructure.Processors;
using EventManager.Persistence;
using EventManager.Persistence.Repositories;
using EventManager.Persistence.UnitOfWork;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<JwtOptions>(
builder.Configuration.GetSection(JwtOptions.JwtOptionsKey));

// ƒобавление Identity дл€ пользовател€ с настройками безопасности парол€
// и уникальности электронной почты,
// и подключение к identity information stores через Entity Framework.
builder.Services.AddIdentityWithPasswordAndEmailSecurity();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString"));
});

builder.Services.AddAutoMapper(
    typeof(EventManager.Persistence.Mapping.EventProfile).Assembly
);

//–егистраци€ в DI контейнер
builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateEventRequestValidator>();

builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//добавл€ет в контейнер DI сервис IHttpContextAccessor, дл€ доступ к HttpContext
builder.Services.AddHttpContextAccessor(); 

var app = builder.Build();

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider("/data/uploads"),
//    RequestPath = "/uploads"
//});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "EventManager");
    });
}


app.UseExceptionHandler(_ => { });

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.AddMappedEndpoints();

app.Run();


