using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task DeleteCategoryAsync(Guid categoryId);
    Task<Guid> AddCategoryAsync(string name);
    Task RenameCategoryAsync(Guid categoryId, string newName);
    Task<IEnumerable<Category>> GetCategoriesAsync();
}
