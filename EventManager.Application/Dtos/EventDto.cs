namespace EventManager.Application.Dtos;

public class EventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public int MaxParticipants { get; set; }
    public int RegisteredParticipants { get; set; }
    public List<string> ImageUrls { get; set; }
    public List<Guid> ParticipantsIds { get; set; }

    // Конструктор по умолчанию, для automapper
    public EventDto() { }
}
