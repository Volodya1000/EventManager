namespace EventManager.Domain.Models;
public class Event
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public DateTime DateTime { get; }
    public string Location { get; }
    public Guid CategoryId { get; }
    public int MaxParticipants { get; }
    public int RegisteredParticipants => _participants.Count;

    private readonly List<string> _imageUrls;
    public IReadOnlyList<string> ImageUrls => _imageUrls.AsReadOnly();

    private readonly List<Participant> _participants;
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    private Event(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        Guid categoryId,
        int maxParticipants,
        List<string> imageUrls,
        List<Participant> participants)
    {
        Id = id;
        Name = name;
        Description = description;
        DateTime = dateTime;
        Location = location;
        CategoryId = categoryId;
        MaxParticipants = maxParticipants;
        _imageUrls = imageUrls ?? new List<string>();
        _participants = new List<Participant>(participants); 
    }

    public static Event Create(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        Guid categoryId,
        int maxParticipants,
        List<string> imageUrls = null,
        List<Participant> initialParticipants = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty", nameof(name));

        if (maxParticipants <= 0)
            throw new ArgumentException("Max participants must be positive", nameof(maxParticipants));

        if (dateTime < DateTime.UtcNow)
            throw new ArgumentException("Event date cannot be in the past", nameof(dateTime));

        var participants = initialParticipants ?? new List<Participant>();
        var images = imageUrls ?? new List<string>();

        if (participants.Count > maxParticipants)
            throw new ArgumentException(
                $"Initial participants count {participants.Count} exceeds max capacity {maxParticipants}",
                nameof(initialParticipants));

        // Проверка уникальности участников
        var duplicateParticipants = participants
            .GroupBy(p => p.UserId)
            .Any(g => g.Count() > 1);

        if (duplicateParticipants)
            throw new ArgumentException("Duplicate participants detected", nameof(initialParticipants));

        return new Event(
            id,
            name.Trim(),
            description?.Trim(),
            dateTime,
            location.Trim(),
            categoryId,
            maxParticipants,
            images,
            participants);
    }

    public void AddParticipant(Participant participant)
    {
        if (_participants.Count >= MaxParticipants)
            throw new InvalidOperationException("Event is full");

        if (_participants.Any(p => p.UserId == participant.UserId))
            throw new InvalidOperationException("Participant already registered");

        _participants.Add(participant);
    }

    public bool RemoveParticipant(Guid userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        return participant != null && _participants.Remove(participant);
    }
}

