namespace EventManager.Persistence.Entities;

public class CategoryEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<EventEntity> Events { get; set; } = new List<EventEntity>();
}