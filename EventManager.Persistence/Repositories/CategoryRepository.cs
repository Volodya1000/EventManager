using AutoMapper;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence.Repositories;

public class CategoryRepository: ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddCategoryAsync(string name)
    {
        if (await _context.Categories.AnyAsync(c => c.Name == name))
            throw new InvalidOperationException("Category with this name already exists");

        var newCategory = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        await _context.Categories.AddAsync(newCategory);
        await _context.SaveChangesAsync();

        return newCategory.Id;
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);

        if (category == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        // Проверка, что категория не используется в событиях
        if (await _context.Events.AnyAsync(e => e.CategoryId == categoryId))
        {
            throw new InvalidOperationException("Cannot delete category because it is used in events");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

    public async Task RenameCategoryAsync(Guid categoryId, string newName)
    {
        var category = await _context.Categories.FindAsync(categoryId);

        if (category == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        // Проверка на уникальность нового имени категории
        if (await _context.Categories.AnyAsync(c => c.Name == newName && c.Id != categoryId))
            throw new InvalidOperationException("Category with this name already exists");

        category.Name = newName;
        await _context.SaveChangesAsync();
    }
}
