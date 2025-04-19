using EventManager.Application.Interfaces.Services;
using global::EventManager.Domain.Constants;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace EventManager.Api.Endpoints;

public static class ImagesEndpoints
{
    public static IEndpointRouteBuilder MapImagesEndpoints(this IEndpointRouteBuilder app)
    {
        var imageGroup = app.MapGroup("/api/events/{eventId}/images")
            .WithTags("Event Images")
            .WithOpenApi();

        imageGroup.MapGet("/{filename}", GetEventImage)
            .Produces<FileContentResult>();

        imageGroup.MapPost("/", UploadEventImage)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<string>(StatusCodes.Status201Created)
            .WithMetadata(new DisableAntiforgeryAttribute());

        imageGroup.MapDelete("/{filename}", DeleteEventImage)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> GetEventImage(
        Guid eventId,
        string filename,
        [FromServices] IImageService imageService)
    {
        var imageData = await imageService.GetImageAsync(eventId, filename);
        return Results.File(imageData.Bytes, imageData.MimeType);
    }

    private static async Task<IResult> UploadEventImage(
        Guid eventId,
        [FromForm] IFormFile image,
        [FromServices] IImageService imageService)
    {
        var imageUrl = await imageService.UploadImageAsync(eventId, image);
        return Results.Created($"/api/events/{eventId}/images/{Path.GetFileName(imageUrl)}", imageUrl);
    }

    private static async Task<IResult> DeleteEventImage(
        Guid eventId,
        string filename,
        [FromServices] IImageService imageService)
    {
        await imageService.DeleteImageAsync(eventId, filename);
        return Results.NoContent();
    }
}

public class DisableAntiforgeryAttribute : Attribute, IAntiforgeryMetadata
{
    public bool RequiresValidation => false;
}