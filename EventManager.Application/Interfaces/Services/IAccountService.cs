using EventManager.Application.Requests;

namespace EventManager.Application.Interfaces.Services;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequest registerRequest, CancellationToken cst = default);
    Task LoginAsync(LoginRequest loginRequest, CancellationToken cst = default);
    Task RefreshTokenAsync(string? refreshToken, CancellationToken cst = default);
    Task PromoteUserToAdminAsync(string email, CancellationToken cst = default);
    Guid GetCurrentUserId();
}
