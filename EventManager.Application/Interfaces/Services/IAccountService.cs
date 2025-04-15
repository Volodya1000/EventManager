using EventManager.Application.Requests;
using Microsoft.AspNetCore.Http;

namespace EventManager.Application.Interfaces.Services;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequest registerRequest);
    Task LoginAsync(LoginRequest loginRequest);
    Task RefreshTokenAsync(string? refreshToken);
    Task PromoteUserToAdminAsync(string email);
    Guid? GetUserIdFromToken();

}