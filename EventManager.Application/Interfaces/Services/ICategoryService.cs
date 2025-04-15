using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<Guid> AddCategoryAsync(string name);
    Task DeleteCategoryAsync(Guid categoryId);
    Task RenameCategoryAsync(Guid categoryId, string newName);
    Task<IEnumerable<Category>> GetCategoriesAsync();
}