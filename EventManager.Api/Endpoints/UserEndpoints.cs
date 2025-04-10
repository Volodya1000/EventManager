
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Requests;

namespace EventManager.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("/api/account");

        accountGroup.MapPost("/register", Register);
        accountGroup.MapPost("/login", Login);
        accountGroup.MapPost("/refresh", RefreshToken);

        return app;
    }

    private static async Task<IResult> Register(RegisterRequest registerRequest, IAccountService accountService)
    {
        await accountService.RegisterAsync(registerRequest);

        return Results.Ok();
    }

    private static async Task<IResult> Login(LoginRequest loginRequest, IAccountService accountService)
    {
        await accountService.LoginAsync(loginRequest);

        return Results.Ok();
    }

    private static async Task<IResult> RefreshToken(HttpContext httpContext, IAccountService accountService)
    {
        var refreshToken = httpContext.Request.Cookies["REFRESH_TOKEN"];

        await accountService.RefreshTokenAsync(refreshToken);

        return Results.Ok();
    }
}
