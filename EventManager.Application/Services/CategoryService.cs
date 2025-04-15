using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Models;

namespace EventManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _repository.GetCategoriesAsync();
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        await _repository.DeleteCategoryAsync(categoryId);
    }
    public async Task<Guid> AddCategoryAsync(string name)
    {
        return await _repository.AddCategoryAsync(name);
    }

    public async Task RenameCategoryAsync(Guid categoryId, string newName)
    {
        await _repository.RenameCategoryAsync(categoryId, newName);
    }
}
