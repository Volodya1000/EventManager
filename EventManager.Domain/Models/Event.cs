namespace EventManager.Domain.Models;

public class Event
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime DateTime { get; private set; }
    public string Location { get; private set; }
    public int CategoryId { get; private set; } 
    public int MaxParticipants { get; private set; }
    public string ImageUrl { get; private set; }

    // Приватный список ID участников для инкапсуляции
    private readonly List<int> _participantIds = new();
    public IReadOnlyCollection<int> ParticipantIds => _participantIds.AsReadOnly();


    public void AddParticipant(int participantId)
    {
        if (_participantIds.Count >= MaxParticipants)
            throw new InvalidOperationException("The event contains the maximum number of participants.");

        _participantIds.Add(participantId);
    }

    public void RemoveParticipant(int participantId)
    {
        _participantIds.Remove(participantId);
    }

    public Event(
        string name,
        string description,
        DateTime dateTime,
        string location,
        int categoryId,
        int maxParticipants,
        string imageUrl)
    {
        Name = name;
        Description = description;
        DateTime = dateTime;
        Location = location;
        CategoryId = categoryId;
        MaxParticipants = maxParticipants;
        ImageUrl = imageUrl;
    }
}
