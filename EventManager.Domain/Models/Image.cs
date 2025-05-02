namespace EventManager.Domain.Models;

public class Image
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Url { get; private set; }

    private Image() { }

    private Image(Guid id, Guid eventId, string url)
    {
        Id = id;
        EventId = eventId;
        Url = url;
    }

    public static Image Create(Guid eventId, string url)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId не может быть пустым", nameof(eventId));
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL не может быть пустым", nameof(url));

        return new Image(Guid.NewGuid(), eventId, url);
    }
}
