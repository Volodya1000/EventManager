using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Application.Services;
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
        [FromServices] IAccountService accountService,
        [FromServices] IEventService eventService,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = accountService.GetUserIdFromToken();
        if (!userId.HasValue) return Results.Forbid();

        try
        {
            var participationId = await eventService.RegisterAsync(eventId, userId.Value);
            return Results.Created($"/api/events/{eventId}/participants/{participationId}", participationId);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> GetEventParticipants(
        Guid eventId,
        [FromServices] IEventService service,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var result = await service.GetParticipantsAsync(eventId, pageNumber, pageSize);
        return Results.Ok(result);
    }

    private static async Task<IResult> CancelMyParticipation(
        Guid eventId,
        [FromServices] IAccountService accountService,
        [FromServices] IEventService eventService,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = accountService.GetUserIdFromToken();
        if (!userId.HasValue) return Results.Forbid();

        try
        {
            await eventService.CancelAsync(eventId, userId.Value);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found")
                ? Results.NotFound(ex.Message)
                : Results.BadRequest(ex.Message);
        }
    }
}