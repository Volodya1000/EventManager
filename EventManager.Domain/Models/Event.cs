namespace EventManager.Domain.Models;
public class Event
{
    public Guid Id { get; }
    public string Name { get;  }
    public string Description { get; private set; }
    public DateTime DateTime { get; private set; }
    public string Location { get; private set; }
    public string  Category { get; }
    public int MaxParticipants { get; private set; }
    public int RegisteredParticipants => _participants.Count;

    private readonly List<string> _imageUrls;
    public IReadOnlyList<string> ImageUrls => _imageUrls.AsReadOnly();

    private readonly List<Participant> _participants = new();
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();

    private Event(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        string category,
        int maxParticipants,
        List<string> imageUrls)
    {
        Id = id;
        Name = name;
        Description = description;
        DateTime = dateTime;
        Location = location;
        Category = category;
        MaxParticipants = maxParticipants;
        _imageUrls = imageUrls ?? new List<string>();
    }

    public static Event Create(
        Guid id,
        string name,
        string description,
        DateTime dateTime,
        string location,
        string category,
        int maxParticipants,
        List<string> imageUrls = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Event name cannot be empty", nameof(name));

        if (maxParticipants <= 0)
            throw new ArgumentException("Max participants must be positive", nameof(maxParticipants));

        if (dateTime < DateTime.UtcNow)
            throw new ArgumentException("Event date cannot be in the past", nameof(dateTime));

        return new Event(
           id,
           name.Trim(),
           description?.Trim(),
           dateTime,
           location.Trim(),
           category,
           maxParticipants,
           imageUrls);

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


    public void UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new ArgumentException("Description cannot be empty or whitespace.", nameof(newDescription));

        Description = newDescription.Trim();
    }

    public void UpdateDateTime(DateTime newDateTime)
    {
        if (newDateTime < DateTime.UtcNow)
            throw new ArgumentException("Event date cannot be in the past.", nameof(newDateTime));

        DateTime = newDateTime;
    }

    public void UpdateLocation(string newLocation)
    {
        if (string.IsNullOrWhiteSpace(newLocation))
            throw new ArgumentException("Location cannot be empty or whitespace.", nameof(newLocation));

        Location = newLocation.Trim();
    }

    public void UpdateMaxParticipants(int newMax)
    {
        if (newMax <= 0)
            throw new ArgumentException("Max participants must be positive.", nameof(newMax));

        if (newMax < RegisteredParticipants)
            throw new ArgumentException("New max participants cannot be less than registered participants.", nameof(newMax));

        MaxParticipants = newMax;
    }

    public void AddImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty or whitespace.", nameof(url));

        var trimmedUrl = url.Trim();
        if (!Uri.IsWellFormedUriString(trimmedUrl, UriKind.Absolute))
            throw new ArgumentException("Invalid URL format.", nameof(url));

        _imageUrls.Add(trimmedUrl);
    }
}

