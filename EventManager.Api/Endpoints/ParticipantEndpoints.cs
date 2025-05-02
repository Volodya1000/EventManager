using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

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
            .Produces(StatusCodes.Status201Created);

        participantGroup.MapGet("/", GetEventParticipants)
            .Produces<PagedResponse<ParticipantDto>>();

        participantGroup.MapDelete("/", CancelMyParticipation)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Detects user from JWT",
            })
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> RegisterParticipant(
      Guid eventId,
      [FromServices] IParticipantService participantService,
      CancellationToken cancellationToken)
    {
        var participationId = await participantService.RegisterAsync(eventId, cancellationToken);
        return Results.Created($"/api/events/{eventId}/participants/{participationId}", participationId);
    }

    private static async Task<IResult> GetEventParticipants(
        Guid eventId,
        [FromServices] IParticipantService service,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetParticipantsAsync(eventId, pageNumber, pageSize, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CancelMyParticipation(
        Guid eventId,
        [FromServices] IParticipantService participantService,
        CancellationToken cancellationToken)
    {
        await participantService.CancelAsync(eventId, cancellationToken);
        return Results.NoContent();
    }
}