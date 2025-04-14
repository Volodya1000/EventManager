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
               Summary = "Creates a category whose name must be unique. Requires admin permissions",
            });

        categoriesGroup.MapDelete("/{id}", DeleteCategory)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Deleting an existing category.Only if there are no events associated with it. Requires admin permissions",
            });

        categoriesGroup.MapPut("/{id}/rename", RenameCategory)
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Update name of an existing category. Requires admin permissions",
            });

        return app;
    }

    private static async Task<IResult> CreateCategory(
        string name,        
        [FromServices] ICategoryService service)
    {
        var createdId = await service.AddCategoryAsync(name);
        return Results.Created($"/api/events/categories/{createdId}", createdId);
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        [FromServices] ICategoryService service)
    {
        await service.DeleteCategoryAsync(id);
        return Results.NoContent();
    }

    private static async Task<IResult> RenameCategory(
        Guid id,
        string newName,
        [FromServices] ICategoryService service)
    {
        await service.RenameCategoryAsync(id, newName);
        return Results.NoContent();
       
    }
}