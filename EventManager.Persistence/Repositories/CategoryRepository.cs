using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence.Repositories;

public class CategoryRepository: ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoryRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        return _mapper.Map<List<Category>>(categories);
    }

    public async Task<Guid> AddCategoryAsync(Category category)
    {
        var entity = _mapper.Map<CategoryEntity>(category);

        await _context.Categories.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity.Id;
    }

    public async Task DeleteCategoryAsync(Category category)
    {
        var entity = await _context.Categories.FindAsync(category.Id);

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        var existingEntity = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);

        _mapper.Map(category, existingEntity); 
        _context.Entry(existingEntity).State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string categoryName)
    {
        return await _context.Categories.AsNoTracking()
            .AnyAsync(c=>c.Name == categoryName);
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Categories.FindAsync(id);
        return _mapper.Map<Category?>(entity);
    }
}
