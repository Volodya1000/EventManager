using EventManager.Application.Dtos;
using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Constants;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace EventManager.Api.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var eventGroup = app.MapGroup("/api/events")
            .WithTags("Events")
            .WithOpenApi();

        eventGroup.MapGet("/", GetAllEvents)
            .Produces<List<EventDto>>();

        eventGroup.MapGet("/{id}", GetEventById)
            .Produces<EventDto>()
            .Produces(StatusCodes.Status404NotFound);

        eventGroup.MapPost("/", CreateEvent)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Accepts<CreateEventRequest>("application/json")
            .Produces(StatusCodes.Status201Created);

        eventGroup.MapPut("/{id}", UpdateEvent)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        eventGroup.MapDelete("/{id}", DeleteEvent)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        eventGroup.MapPost("/filter", GetFilteredEvents)
            .Produces<List<EventDto>>()
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Filter events by criteria",
            });

        eventGroup.MapGet("/user/{userId}", GetEventsByUser)
           .RequireAuthorization()
           .Produces(StatusCodes.Status404NotFound)
           .WithOpenApi(operation => new OpenApiOperation(operation)
           {
               Summary = "Receiving events in which the user is a participant Required Authorization",
           });

        return app;
    }

    private static async Task<IResult> GetAllEvents(
        [FromServices] IEventService service,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await service.GetAllAsync(page, pageSize);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetEventById(
        Guid id,
        IEventService service)
    {
        var result = await service.GetByIdAsync(id);
        return result is not null ? Results.Ok(result) : Results.NotFound();
    }

    private static async Task<IResult> CreateEvent(
        [FromServices] IEventService service,
        [FromBody] CreateEventRequest request)
    {
        var createdId = await service.CreateAsync(request);
        return Results.Created($"/api/events/{createdId}", createdId);
    }

    private static async Task<IResult> UpdateEvent(
        Guid id,
        [FromBody] UpdateEventRequest request,
        IEventService service)
    {
        await service.UpdateAsync(id, request);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteEvent(
        Guid id,
        IEventService service)
    {
        await service.DeleteAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> GetFilteredEvents(
        [FromServices] IEventService service,
        [FromBody] EventFilterRequest eventFilterRequest,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await service.GetFilteredAsync(eventFilterRequest, page, pageSize);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetEventsByUser(
        [FromServices] IEventService eventService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await eventService.GetEventsByUserAsync(page, pageSize);
        return Results.Ok(result);
    }
}