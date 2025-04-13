namespace EventManager.Persistence.Entities;

//здесь потенциально могут быть дополнительные метаданные об изображении
internal class ImageEntity
{
    public Guid Id { get; set; }
    public required string Url { get; set; }
}