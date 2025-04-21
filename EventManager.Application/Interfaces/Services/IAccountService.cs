using EventManager.Application.Requests;

namespace EventManager.Application.Interfaces.Services;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequest registerRequest, CancellationToken cancellationToken = default);
    Task LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);
    Task RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default);
    Task PromoteUserToAdminAsync(string email, CancellationToken cancellationToken = default);
    Guid GetCurrentUserId();
}
