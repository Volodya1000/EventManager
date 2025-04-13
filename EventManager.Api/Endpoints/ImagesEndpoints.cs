
using global::EventManager.Application.Interfaces.Services;
using global::EventManager.Domain.Constants;
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
            .Produces<FileStreamHttpResult>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Get event image by filename"
            });

        imageGroup.MapPost("/", UploadEventImage)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<string>()
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
            // Проверка валидности имени файла
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Invalid filename");

            // Защита от path traversal attacks
            if (filename.Contains("..") || Path.IsPathRooted(filename))
                return Results.BadRequest("Invalid filename format");

            // Получение полного пути к файлу
            var filePath = Path.Combine(
                Environment.GetEnvironmentVariable("FILE_STORAGE_PATH") ?? "/data/uploads",
                filename);

            // Проверка существования файла
            if (!System.IO.File.Exists(filePath))
                return Results.NotFound("Image not found");

            // Определение MIME-типа
            var mimeType = GetMimeType(filename);

            return Results.File(filePath, mimeType);
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
        IFormFile image,
        [FromServices] IImageService imageService)
    {
        var imageUrl = await imageService.UploadEventImageAsync(eventId, image);
        return Results.Created($"/api/events/{eventId}/images/{Path.GetFileName(imageUrl)}", imageUrl);
    }

    private static async Task<IResult> DeleteEventImage(
        Guid eventId,
        string filename,
        [FromServices] IImageService imageService)
    {
        await imageService.DeleteEventImageAsync(eventId, filename);
        return Results.NoContent();
    }
}