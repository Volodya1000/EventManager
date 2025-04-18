namespace EventManager.Domain.Models;

public class Category
{
    public Guid Id { get; init; }
    public string Name { get; private set; }

    public static Category Create(Guid id, string name)
    {
        return new Category
        {
            Id = id,
            Name = name
        };
    }

    public void Rename(string newName)
    {
        Name= newName;
    }
}