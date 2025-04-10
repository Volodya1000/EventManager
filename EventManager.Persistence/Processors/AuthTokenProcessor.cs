using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventManager.Domain.Options;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using EventManager.Application.Interfaces.AuthTokenProcessor;

namespace EventManager.Infrastructure.Processors;

public class AuthTokenProcessor : IAuthTokenProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtOptions _jwtOptions;
    
    public AuthTokenProcessor(IOptions<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtOptions = jwtOptions.Value;
    }

    public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user)
    {
        // Создание симметричного ключа безопасности из секретного ключа JWT.
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        // Настройка учетных данных для подписи токена.
        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        // Создание массива claims, которые будут включены в токен.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Идентификатор субъекта
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Уникальный идентификатор токена
            new Claim(JwtRegisteredClaimNames.Email, user.Email), // Адрес электронной почты
            new Claim(ClaimTypes.NameIdentifier, user.ToString())// Идентификатор имени В ToString() Имя + Фамилия
        };

        // Вычисление времени истечения срока действия токена
        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationTimeInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        // Запись токена в строку
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expires);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    //Метод для сохранения токена в HttpOnlyCookie, это наиболее безопасный вариант
    public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token,
        DateTime expiration)
    {
        _httpContextAccessor.HttpContext.Response.Cookies.Append(cookieName,
            token, new CookieOptions
            {
                HttpOnly = true,
                Expires = expiration,
                //указывает cookie является необходимым
                //В General Data Protection Regulation это означает,
                //что cookie можно использовать без получения согласия пользователя,
                //поскольку он строго необходим для функционирования сайта или приложения
                IsEssential = true,
                Secure = true,// будет передаваться только по HTTPS
                //cookie будет отправляться только тогда,
                //когда пользователь находится на том же сайте, где cookie был установлен
                //Это предотвращает атаки Cross-Site Request Forgery
                SameSite = SameSiteMode.Strict 
            });
    }
}