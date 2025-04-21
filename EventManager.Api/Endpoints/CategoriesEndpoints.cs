using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Constants;
using EventManager.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var categoriesGroup = app.MapGroup("/api/events/categories")
            .WithTags("Categories")
            .WithOpenApi();

        // Эндпоинт получения всех категорий без авторизации
        categoriesGroup.MapGet("/", GetAllCategories)
            .Produces<IEnumerable<Category>>(StatusCodes.Status200OK)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Retrieves all categories. No authentication required",
            });

        var adminGroup = categoriesGroup.MapGroup("")
            .RequireAuthorization(policy => policy.RequireRole(IdentityRoleConstants.Admin));

        adminGroup.MapPost("/", CreateCategory)
            .Produces(StatusCodes.Status201Created)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Creates a category which name must be unique. Requires admin permissions",
            });

        adminGroup.MapDelete("/{id}", DeleteCategory)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Deletes an existing category only if there are no events associated with it. Requires admin permissions",
            });

        adminGroup.MapPut("/{id}/rename", RenameCategory)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Updates the name of an existing category. Requires admin permissions",
            });

        return app;
    }

    private static async Task<IResult> GetAllCategories(
       [FromServices] ICategoryService service,
       CancellationToken cst) 
    {
        var categories = await service.GetCategoriesAsync(cst); 
        return Results.Ok(categories);
    }

    private static async Task<IResult> CreateCategory(
        string name,
        [FromServices] ICategoryService service,
        CancellationToken cst) 
    {
        var createdId = await service.AddCategoryAsync(name, cst); 
        return Results.Created($"/api/events/categories/{createdId}", createdId);
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        [FromServices] ICategoryService service,
        CancellationToken cst)
    {
        await service.DeleteCategoryAsync(id, cst); 
        return Results.NoContent();
    }

    private static async Task<IResult> RenameCategory(
        Guid id,
        string newName,
        [FromServices] ICategoryService service,
        CancellationToken cst) 
    {
        await service.RenameCategoryAsync(id, newName, cst); 
        return Results.NoContent();
    }
}