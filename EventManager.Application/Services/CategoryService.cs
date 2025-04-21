using EventManager.Domain.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Models;

namespace EventManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEventRepository _eventRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IEventRepository eventRepository)
    {
        _categoryRepository = categoryRepository;
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _categoryRepository.GetCategoriesAsync();
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        if (await _eventRepository.AnyEventWithCategoryAsync(categoryId))
            throw new InvalidOperationException("Cannot delete category used in events");

        await _categoryRepository.DeleteCategoryAsync(category);
    }

    public async Task<Guid> AddCategoryAsync(string name)
    {
        if (await _categoryRepository.ExistsAsync(name))
            throw new InvalidOperationException("Category with this name already exists");

        var category = Category.Create(Guid.NewGuid(),name);
        return await _categoryRepository.AddCategoryAsync(category);
    }

    public async Task RenameCategoryAsync(Guid categoryId, string newName)
    {
        if (await _categoryRepository.ExistsAsync(newName))
            throw new InvalidOperationException("Category with this name already exists");

        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        category.Rename(newName);
        await _categoryRepository.UpdateCategoryAsync(category);
    }
}
