using EventManager.Domain.Interfaces.Repositories;
using EventManager.Application.Interfaces.Services;
using EventManager.Domain.Models;
using EventManager.Application.Exceptions;

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

    public async Task<IEnumerable<Category>> GetCategoriesAsync(
        CancellationToken cst = default)
    {
        return await _categoryRepository.GetCategoriesAsync(cst);
    }

    public async Task DeleteCategoryAsync(
        Guid categoryId,
        CancellationToken cst = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cst);
        if (category == null)
            throw new NotFoundException("Category not found");

        if (await _eventRepository.AnyEventWithCategoryAsync(categoryId, cst))
            throw new InvalidOperationException("Cannot delete category used in events");

        await _categoryRepository.DeleteCategoryAsync(category, cst);
    }

    public async Task<Guid> AddCategoryAsync(
        string name,
        CancellationToken cst = default)
    {
        if (await _categoryRepository.ExistsAsync(name, cst))
            throw new InvalidOperationException("Category with this name already exists");

        var category = Category.Create(Guid.NewGuid(), name);
        return await _categoryRepository.AddCategoryAsync(category, cst);
    }

    public async Task RenameCategoryAsync(
        Guid categoryId,
        string newName,
        CancellationToken cst = default)
    {
        if (await _categoryRepository.ExistsAsync(newName, cst))
            throw new InvalidOperationException("Category with this name already exists");

        var category = await _categoryRepository.GetByIdAsync(categoryId, cst);
        if (category == null)
            throw new NotFoundException("Category not found");

        category.Rename(newName);
        await _categoryRepository.UpdateCategoryAsync(category, cst);
    }
}
