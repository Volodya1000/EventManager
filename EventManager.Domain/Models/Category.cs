namespace EventManager.Domain.Models;

public class Category
{
    public Guid Id { get; init; }
    public string Name { get; init; }

    public static Category Create(Guid id, string name)
    {
        return new Category
        {
            Id = id,
            Name = name
        };
    }
}