namespace EventManager.Persistence.Entities;

public class ImageEntity
{
    public Guid Id { get; set; }
    public required string Url { get; set; }
}