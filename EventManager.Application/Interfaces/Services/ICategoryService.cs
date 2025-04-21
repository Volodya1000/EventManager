using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetCategoriesAsync(
        CancellationToken cst = default);

    Task DeleteCategoryAsync(
        Guid categoryId,
        CancellationToken cst = default);

    Task<Guid> AddCategoryAsync(
        string name,
        CancellationToken cst = default);

    Task RenameCategoryAsync(
        Guid categoryId,
        string newName,
        CancellationToken cst = default);
}