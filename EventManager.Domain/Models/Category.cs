namespace EventManager.Domain.Models;

public class Category
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    public Category(string name)
    {
        Name = name;
    }
}