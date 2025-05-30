﻿
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Constants;
using EventManager.Application.Requests;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var accountGroup = app.MapGroup("/api/account");

        accountGroup.MapPost("/register", Register);
        accountGroup.MapPost("/login", Login);
        accountGroup.MapPost("/refresh", RefreshToken);

        app.MapPut("/api/admin/promote/{email}", PromoteToAdmin)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .WithTags("Admin")
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Promote user to admin role. Requires admin permissions",
            })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> Register(
      RegisterRequest registerRequest,
      IAccountService accountService,
      CancellationToken cst)
    {
        await accountService.RegisterAsync(registerRequest, cst);
        return Results.Ok();
    }

    private static async Task<IResult> Login(
        LoginRequest loginRequest,
        IAccountService accountService,
        CancellationToken cst)
    {
        await accountService.LoginAsync(loginRequest, cst);
        return Results.Ok();
    }

    private static async Task<IResult> RefreshToken(
        HttpContext httpContext,
        IAccountService accountService,
        CancellationToken cst)
    {
        var refreshToken = httpContext.Request.Cookies["REFRESH_TOKEN"];
        await accountService.RefreshTokenAsync(refreshToken, cst);
        return Results.Ok();
    }

    private static async Task<IResult> PromoteToAdmin(
        string email,
        IAccountService accountService,
        CancellationToken cst)
    {
        await accountService.PromoteUserToAdminAsync(email, cst);
        return Results.Ok();
    }
}
