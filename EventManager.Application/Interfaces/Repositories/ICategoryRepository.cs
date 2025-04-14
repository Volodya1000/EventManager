namespace EventManager.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    public Task DeleteCategoryAsync(Guid categoryId);
    public Task<Guid> AddCategoryAsync(string name);
    public Task RenameCategoryAsync(Guid categoryId, string newName);
}
