using EventManager.Domain.Models;

namespace EventManager.Domain.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetCategoriesAsync(CancellationToken cst = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cst = default);
    Task<Guid> AddCategoryAsync(Category category, CancellationToken cst = default);
    Task DeleteCategoryAsync(Category category, CancellationToken cst = default);
    Task UpdateCategoryAsync(Category category, CancellationToken cst = default);
    Task<bool> ExistsAsync(string categoryName, CancellationToken cst = default);
}
