using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task<Guid> AddCategoryAsync(Category category);
    Task DeleteCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task<bool> ExistsAsync(string categoryName);
}
