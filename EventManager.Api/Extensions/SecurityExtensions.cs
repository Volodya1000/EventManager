using EventManager.Domain.Models;
using EventManager.Infrastructure.Options;
using EventManager.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EventManager.Api.Extensions;

public static class SecurityExtensions
{
    public static void AddAuthConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.JwtOptionsKey));
        services.AddAuthorization();
    }

    public static void AddApiAuthentication(
  this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//определяет схему вызова аутентификации
            opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            var jwtOptions = configuration.GetRequiredSection(JwtOptions.JwtOptionsKey).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing");

            // Настройка параметров валидации JWT токенов
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
            };

            // Настройка событий обработки токенов JWT
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["ACCESS_TOKEN"];
                    return Task.CompletedTask;
                }
            };
        });
    }

    public static void AddIdentityConfiguration(
    this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<Guid>>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequiredLength = 8;
            opt.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();
    }
}
