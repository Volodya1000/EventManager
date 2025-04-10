using EventManager.Domain.Requests;

namespace EventManager.Application.Interfaces.Services;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequest registerRequest);
    Task LoginAsync(LoginRequest loginRequest);
    Task RefreshTokenAsync(string? refreshToken);
}