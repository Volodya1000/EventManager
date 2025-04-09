namespace EventManager.Domain.Models;

public class Category
{
    public Guid Id { get; init; }
    public string Name { get; init; }

    public Category(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}