namespace EventManager.Persistence.Entities;

internal class CategoryEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}