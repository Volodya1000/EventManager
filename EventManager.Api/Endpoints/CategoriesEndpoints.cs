using EventManager.Application.Interfaces.Services;
using EventManager.Application.Requests;
using EventManager.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace EventManager.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var categoriesGroup = app.MapGroup("/api/events/categories")
            .WithTags("Categories")
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .WithOpenApi();

        categoriesGroup.MapPost("/", CreateCategory)
            .Produces(StatusCodes.Status201Created)
           .WithOpenApi(operation => new OpenApiOperation(operation)
           {
               Summary = "Create new category for events",
               Description = "Only admin endpoint"
           });

        categoriesGroup.MapDelete("/{id}", DeleteCategory)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Only a category that is not related to events can be deleted.",
                Description = "Renaming an existing category"
            });

        categoriesGroup.MapPut("/{id}/rename", RenameCategory)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Renaming an existing category",
                Description = "Renaming an existing category"
            });

        return app;
    }

    private static async Task<IResult> CreateCategory(
        string name,        
        [FromServices] IEventService service)
    {
        var createdId = await service.CreateCategoryAsync(name);
        return Results.Created($"/api/events/categories/{createdId}", createdId);
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        IEventService service)
    {
        await service.DeleteCategoryAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> RenameCategory(
        Guid id,
        string newName,
        [FromServices] IEventService service)
    {
        await service.RenameCategoryAsync(id, newName);
        return Results.NoContent();
       
    }
}