using EventManager.Api.Extensions;
using EventManager.API.Handlers;
using EventManager.Application.Interfaces.AuthTokenProcessor;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Services;
using EventManager.Domain.Constants;
using EventManager.Domain.Models;
using EventManager.Domain.Options;
using EventManager.Infrastructure.Processors;
using EventManager.Persistence;
using EventManager.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

//����������� � DI ���������
builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//��������� � ��������� DI ������ IHttpContextAccessor, ��� ������ � HttpContext
builder.Services.AddHttpContextAccessor(); 

var app = builder.Build();

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

app.MapGet("/api/test-events", () => Results.Ok(new List<string> { "NewYear","First september" }))
    .RequireAuthorization();

app.MapGet("/api/only-admin", () => Results.Ok(new { Message = "Admin information" }))
    .RequireAuthorization(policy => policy.RequireRole(new List<string>
    {
        IdentityRoleConstants.Admin
    }));

app.Run();


