using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventManager.Api.Endpoints;

public static class ParticipantEndpoints
{
    public static IEndpointRouteBuilder MapParticipantEndpoints(this IEndpointRouteBuilder app)
    {
        var participantGroup = app.MapGroup("/api/events/{eventId}/participants")
            .WithTags("Participants")
            .RequireAuthorization();

        participantGroup.MapPost("/", RegisterParticipant)
             .WithOpenApi(operation => new OpenApiOperation(operation)
             {
                 Summary = "Detects user from JWT",
             })
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);

        

        participantGroup.MapGet("/", GetEventParticipants)
            .Produces<PagedResponse<ParticipantDto>>();

        participantGroup.MapDelete("/", CancelMyParticipation)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Detects user from JWT",
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RegisterParticipant(
        Guid eventId,
        IEventService service,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromToken(httpContextAccessor.HttpContext);
        if (!userId.HasValue) return Results.Forbid();

        try
        {
            var participationId = await service.RegisterAsync(eventId, userId.Value);
            return Results.Created($"/api/events/{eventId}/participants/{participationId}", participationId);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> GetEventParticipants(
        Guid eventId,
        IEventService service,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await service.GetParticipantsAsync(eventId, pageNumber, pageSize);
        return Results.Ok(result);
    }

    private static async Task<IResult> CancelMyParticipation(
        Guid eventId,
        IEventService service,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = GetUserIdFromToken(httpContextAccessor.HttpContext);
        if (!userId.HasValue) return Results.Forbid();

        try
        {
            await service.CancelAsync(eventId, userId.Value);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found")
                ? Results.NotFound(ex.Message)
                : Results.BadRequest(ex.Message);
        }
    }

    //Благодаря этой функции зарегистрироваться и отменить регистрацию можно только для себя но, не для других
    private static Guid? GetUserIdFromToken(HttpContext? context)
    {
        var userIdClaim = context?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}