using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Models;
using EventManager.Persistence;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CategoryRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Category>> GetCategoriesAsync(CancellationToken cst = default)
    {
        var categories = await _context.Categories.ToListAsync(cst);
        return _mapper.Map<List<Category>>(categories);
    }

    public async Task<Guid> AddCategoryAsync(Category category, CancellationToken cst = default)
    {
        var entity = _mapper.Map<CategoryEntity>(category);

        await _context.Categories.AddAsync(entity, cst);
        await _context.SaveChangesAsync(cst);

        return entity.Id;
    }

    public async Task DeleteCategoryAsync(Category category, CancellationToken cst = default)
    {
        var entity = await _context.Categories.FindAsync(category.Id, cst);

        _context.Categories.Remove(entity);
        await _context.SaveChangesAsync(cst);
    }

    public async Task UpdateCategoryAsync(Category category, CancellationToken cst = default)
    {
        var existingEntity = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id, cst);

        _mapper.Map(category, existingEntity);
        _context.Entry(existingEntity).State = EntityState.Modified;

        await _context.SaveChangesAsync(cst);
    }

    public async Task<bool> ExistsAsync(string categoryName, CancellationToken cst = default)
    {
        return await _context.Categories.AsNoTracking()
            .AnyAsync(c => c.Name == categoryName, cst);
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cst = default)
    {
        var entity = await _context.Categories.FindAsync(id, cst);
        return _mapper.Map<Category?>(entity);
    }
}