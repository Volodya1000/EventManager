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
            .Produces<FileContentResult>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get event image by filename"
            });

        imageGroup.MapPost("/", UploadEventImage)
             .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
             .Accepts<IFormFile>("multipart/form-data")
             .Produces<string>(StatusCodes.Status201Created)
             .WithMetadata(new DisableAntiforgeryAttribute())
             .WithOpenApi(operation => new OpenApiOperation(operation)
             {
                 Summary = "Upload new image for event"
             });

        imageGroup.MapDelete("/{filename}", DeleteEventImage)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Delete event image by filename"
            });

        return app;
    }

    private static async Task<IResult> GetEventImage(
        Guid eventId,
        string filename,
        [FromServices] IImageService imageService)
    {
        try
        {
            var imageBytes = await imageService.GetImageAsync(eventId, filename);
            var mimeType = GetMimeType(filename);
            return Results.File(imageBytes, mimeType);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound("Image not found");
        }
        catch (Exception ex)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private static string GetMimeType(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static async Task<IResult> UploadEventImage(
        Guid eventId,
        [FromForm] IFormFile image,
        [FromServices] IImageService imageService)
    {
        try
        {
            var imageUrl = await imageService.UploadImageAsync(eventId, image);
            return Results.Created(
                $"/api/events/{eventId}/images/{Path.GetFileName(imageUrl)}",
                imageUrl);
        }
        catch (Exception ex)
        {
            return Results.Problem("Error uploading image",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> DeleteEventImage(
        Guid eventId,
        string filename,
        [FromServices] IImageService imageService)
    {
        try
        {
            // Приведение имени файла к корректному формату
            var normalizedFilename = filename.StartsWith("/uploads/")
                ? filename
                : $"/uploads/{filename}";

            await imageService.DeleteImageAsync(eventId, normalizedFilename);
            return Results.NoContent();
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound("Image not found");
        }
        catch (Exception ex)
        {
            return Results.Problem("Error deleting image",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

public class DisableAntiforgeryAttribute : Attribute, IAntiforgeryMetadata
{
    public bool RequiresValidation => false;
}