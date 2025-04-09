namespace EventManager.Domain.Models;

public class Event
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public DateTime DateTime { get; init; }
    public string Location { get; init; }
    public Guid CategoryId { get; init; }
    public int MaxParticipants { get; init; }
    public string ImageUrl { get; init; }

    private readonly List<Guid> _participantIds = new();
    public IReadOnlyCollection<Guid> ParticipantIds => _participantIds.AsReadOnly();

    public void AddParticipant(Guid participantId)
    {
        if (_participantIds.Count >= MaxParticipants)
            throw new InvalidOperationException("The event contains the maximum number of participants.");

        _participantIds.Add(participantId);
    }

    public void RemoveParticipant(Guid participantId)
    {
        _participantIds.Remove(participantId);
    }

    public Event(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        Guid categoryId,
        int maxParticipants,
        string imageUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        DateTime = dateTime;
        Location = location;
        CategoryId = categoryId;
        MaxParticipants = maxParticipants;
        ImageUrl = imageUrl;
    }
}
