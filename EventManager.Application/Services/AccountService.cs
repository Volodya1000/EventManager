using EventManager.Application.Interfaces.AuthTokenProcessor;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Models;
using EventManager.Application.Exceptions;
using EventManager.Application.Requests;
using Microsoft.AspNetCore.Identity;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Constants;
using Microsoft.Extensions.Logging;
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

    public async Task RegisterAsync(RegisterRequest registerRequest, CancellationToken cst = default)
    {
        cst.ThrowIfCancellationRequested();

        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email)
            .WaitAsync(cst)??
            throw new UserAlreadyExistsException(registerRequest.Email);

        var user = User.Create(
            registerRequest.Email,
            registerRequest.FirstName,
            registerRequest.LastName,
            registerRequest.DateOfBirth);

        var result = await _userManager.CreateAsync(user, registerRequest.Password)
            .WaitAsync(cst);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description);
            _logger.LogWarning("Registration failed for {Email}: {Errors}",
                registerRequest.Email, errors);
            throw new RegistrationFailedException(errors);
        }

        await _userManager.AddToRoleAsync(user, IdentityRoleConstants.User)
            .WaitAsync(cst);
    }

    public async Task LoginAsync(LoginRequest loginRequest, CancellationToken cst = default)
    {
        cst.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(loginRequest.Email)
            .WaitAsync(cst);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password)
            .WaitAsync(cst))
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        await UpdateUserTokensAsync(user, cst);
    }

    public async Task RefreshTokenAsync(string? refreshToken, CancellationToken cst = default)
    {
        cst.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new RefreshTokenException("Refresh token is missing.");
        }

        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken, cst);
        if (user == null)
        {
            throw new RefreshTokenException("Unable to retrieve user for refresh token");
        }

        if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
        {
            throw new RefreshTokenException("Refresh token is expired.");
        }

        await UpdateUserTokensAsync(user, cst);
    }

    private async Task UpdateUserTokensAsync(User user, CancellationToken cst = default)
    {
        cst.ThrowIfCancellationRequested();

        IList<string> roles = await _userManager.GetRolesAsync(user)
            .WaitAsync(cst);

        var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user, roles);
        var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();
        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

        await _userManager.UpdateAsync(user)
            .WaitAsync(cst);

        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);
    }

    public async Task PromoteUserToAdminAsync(string email, CancellationToken cst = default)
    {
        cst.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email)
            .WaitAsync(cst);
        if (user == null) throw new UserNotFoundException(email);

        if (await _userManager.IsInRoleAsync(user, IdentityRoleConstants.Admin)
            .WaitAsync(cst))
        {
            throw new UserAlreadyAdminException(email, IdentityRoleConstants.Admin);
        }

        var result = await _userManager.AddToRoleAsync(user, IdentityRoleConstants.Admin)
            .WaitAsync(cst);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Role promotion failed for user with email:{Email}: {Errors}", email, errors);
            throw new PromotionFailedException(email, IdentityRoleConstants.Admin, errors);
        }

        _logger.LogInformation("User with email:{Email} promoted to Admin role", email);
    }

    public Guid GetCurrentUserId()
    {
        var userId = GetUserIdFromToken();
        return userId ?? throw new UnauthorizedException("User not authenticated");
    }

    private Guid? GetUserIdFromToken()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}