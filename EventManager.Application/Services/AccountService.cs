using EventManager.Application.Interfaces.AuthTokenProcessor;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Models;
using EventManager.Application.Exceptions;
using EventManager.Application.Requests;
using Microsoft.AspNetCore.Identity;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Domain.Constants;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace EventManager.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAuthTokenProcessor _authTokenProcessor;
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AccountService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountService(
        IAuthTokenProcessor authTokenProcessor,
        UserManager<User> userManager,
        IUserRepository userRepository,
        ILogger<AccountService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _authTokenProcessor = authTokenProcessor;
        _userManager = userManager;
        _userRepository = userRepository;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task RegisterAsync(RegisterRequest registerRequest)
    {
        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;

        if (userExists)
        {
            throw new UserAlreadyExistsException(email: registerRequest.Email);
        }

        var user = User.Create(registerRequest.Email, 
            registerRequest.FirstName, registerRequest.LastName, registerRequest.DateOfBirth);


        //Эта перегрузка CreateAsync осуществляет валидацию
        //пароля на соответствие правилам которые описаны
        //в EventManager.Api.Extensions.ApiExtensions.AddIdentityWithPasswordAndEmailSecurity
        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description);
            _logger.LogWarning("Registration failed for {Email}: {Errors}",
                registerRequest.Email, errors);
            throw new RegistrationFailedException(result.Errors.Select(x => x.Description));
        }

        await _userManager.AddToRoleAsync(user, IdentityRoleConstants.User);
    }

    public async Task LoginAsync(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        await UpdateUserTokensAsync(user);
    }

    public async Task RefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new RefreshTokenException("Refresh token is missing.");
        }

        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);

        if (user == null)
        {
            throw new RefreshTokenException("Unable to retrieve user for refresh token");
        }

        if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
        {
            throw new RefreshTokenException("Refresh token is expired.");
        }

        await UpdateUserTokensAsync(user);
    }

    private async Task UpdateUserTokensAsync(User user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);

        var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
        var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();

        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

        await _userManager.UpdateAsync(user);

        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);
    }

    public async Task PromoteUserToAdminAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) throw new UserNotFoundException(email);

        if (await _userManager.IsInRoleAsync(user, IdentityRoleConstants.Admin))
        {
            throw new UserAlreadyAdminException(email, IdentityRoleConstants.Admin);
        }

        var result = await _userManager.AddToRoleAsync(user, IdentityRoleConstants.Admin);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Role promotion failed for user with email:{Email}: {Errors}", email, errors);
            throw new PromotionFailedException(email,IdentityRoleConstants.Admin,errors);
        }

        // Получаем оставшееся время текущего токена
        TimeSpan remainingTime = user.RefreshTokenExpiresAtUtc.HasValue
            ? user.RefreshTokenExpiresAtUtc.Value - DateTime.UtcNow
            : TimeSpan.FromDays(7); // Если токена нет, используем дефолт

        _logger.LogInformation("User with email:{Email} promoted to Admin role", email);
    }

    public Guid? GetUserIdFromToken()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}