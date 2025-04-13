using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Api.Endpoints;

public static class ParticipantEndpoints
{
    public static IEndpointRouteBuilder MapParticipantEndpoints(this IEndpointRouteBuilder app)
    {
        var participantGroup = app.MapGroup("/api/events/{eventId}/participants")
            .WithTags("Participants")
            .RequireAuthorization();

        participantGroup.MapPost("/", RegisterParticipant)
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        participantGroup.MapGet("/", GetEventParticipants)
            .Produces<List<ParticipantDto>>();

        participantGroup.MapDelete("/{participantId}", CancelParticipation)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden);

        return app;
    }

    private static async Task<IResult> RegisterParticipant(
        Guid eventId,
        [FromBody] RegisterParticipantRequest request,
        IEventService service)
    {
        var participationId = await service.RegisterAsync(eventId, request);
        return Results.Created($"/api/events/{eventId}/participants/{participationId}", participationId);
    }

    private static async Task<IResult> GetEventParticipants(
        Guid eventId,
        IEventService service)
    {
        var result = await service.GetParticipantsAsync(eventId);
        return Results.Ok(result);
    }

    private static async Task<IResult> CancelParticipation(
        Guid eventId,
        Guid participantId,
        IEventService service)
    {
        await service.CancelAsync(eventId, participantId);
        return Results.NoContent();
    }
}